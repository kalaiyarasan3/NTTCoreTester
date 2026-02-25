using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTTCoreTester.Configuration;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Reporting;
using NTTCoreTester.Validators;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NTTCoreTester.Services
{
    public interface IApiService
    {

        Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ApiConfiguration _config;
        private readonly ResponseChecker _checker;
        private readonly ActivityExecutor _activityExecutor; 
        private readonly PlaceholderCache _cache;
        private readonly CsvReport _csvReport;

        public ApiService(HttpClient http, ApiConfiguration config, ResponseChecker checker,
                          PlaceholderCache cache, ActivityExecutor activityExecutor, CsvReport csvReport)
        {
            _http = http;
            _config = config;
            _checker = checker;
            _http.BaseAddress = new Uri(_config.BaseUrl);
            _activityExecutor = activityExecutor;
            _cache = cache;
            _csvReport = csvReport;
        }

        public async Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest)
        {
            $"\n{new string('=', 80)}".Debug();
            $"Executing Api: {configRequest.Endpoint}".Debug();
            $"{new string('=', 80)}".Debug();


            string requestJson = JsonConvert.SerializeObject(configRequest.Payload);
            $" Payload loaded from config".Debug();

            var variableResult = _cache.ReplaceVariables(requestJson);

            if (!variableResult.IsSuccess)
            {
                $" Failed to resolve placeholders: {variableResult.Error}".Error(); 
                return false;
            }

            requestJson = variableResult.Text;


            Dictionary<string, string> resolvedHeaders = null;
            if (configRequest.Headers != null && configRequest.Headers.Count > 0)
            {
                resolvedHeaders = new Dictionary<string, string>();
                foreach (var header in configRequest.Headers)
                {
                    var headerVariable = _cache.ReplaceVariables(header.Value);
                    if (!headerVariable.IsSuccess)
                    {
                       $" Failed to resolve placeholder in header: {header.Key}".Error();
                        return false;
                    }
                    resolvedHeaders[header.Key] = headerVariable.Text;
                }
            }

            $" Placeholders resolved".Debug();

            return await CallApi(configRequest.Endpoint, requestJson, resolvedHeaders, configRequest.Activity);
        }

        private async Task<bool> CallApi(
            string endpoint,
            string requestJson,
            Dictionary<string, string> headers,
            string activity)
        {
            try
            {
                var result = await SendRequest(endpoint, requestJson, headers);

                var validation = _checker.Validate(result);

                ActivityResult activityResult = ActivityResult.Success();

                if (validation.IsSuccess && !string.IsNullOrEmpty(activity))
                {
                   $"\n Executing activity: {activity}".Debug();
                    activityResult = _activityExecutor.Execute(
                        activity,
                        result,
                        result.Endpoint);
                }

                bool finalSuccess = validation.IsSuccess && activityResult.IsSuccess;

                if (!string.IsNullOrWhiteSpace(activityResult.Message))
                    validation.Message = activityResult.Message;

                _csvReport.AddEntry(
                    result.Endpoint,
                    result.ResponseTime,
                    result.StatusCode,
                    validation.BusinessStatus,
                    result.ResponseBody,
                    validation.IsSchemaValid,
                    string.Join("; ", validation.Errors),
                    validation.Message);

                if (!validation.IsSuccess)
                    return false;

                if (!activityResult.IsSuccess)
                {
                    if (!activityResult.ContinueExecution)
                        return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                $" ERROR in Api service: {ex.Message}".Error();
                return false;
            }
        }

        private async Task<ApiExecutionResult> SendRequest(
             string endpoint,
             string requestJson,
             Dictionary<string, string> headers)
        {
            try
            {
                var timer = Stopwatch.StartNew();

                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}{endpoint}";
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                foreach (var header in _config.DefaultHeaders)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Remove(header.Key);
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                timer.Restart();
                var response = await _http.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                timer.Stop();

                return new ApiExecutionResult
                {
                    Endpoint = endpoint,
                    StatusCode = (int)response.StatusCode,
                    ResponseBody = body,
                    ResponseTime = timer.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                ex.ToString().Error();
                throw;
            }

        }


    }
}
//90470544
//