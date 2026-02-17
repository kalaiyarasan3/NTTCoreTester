using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using NTTCoreTester.Validators;
using NTTCoreTester.Reporting;
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
        private readonly CsvReport _csvReport;
        private readonly ActivityExecutor _activityExecutor;
        private readonly PlaceholderResolver _resolver;

        public ApiService(HttpClient http, ApiConfiguration config, ResponseChecker checker,
                         CsvReport csvReport, PlaceholderCache cache, ActivityExecutor activityExecutor, PlaceholderResolver resolver)
        {
            _http = http;
            _config = config;
            _checker = checker;
            _csvReport = csvReport;
            _http.BaseAddress = new Uri(_config.BaseUrl);
            _activityExecutor = activityExecutor;
            _resolver = resolver;
        }

        public async Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest)
        {
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine($"Executing: {configRequest.Endpoint}");
            Console.WriteLine($"{new string('=', 80)}");


            string requestJson = JsonConvert.SerializeObject(configRequest.Payload);
            Console.WriteLine($" Payload loaded from config");


            requestJson = await _resolver.ResolvePlaceholders(requestJson, configRequest.Endpoint);
            if (requestJson == null)
            {
                Console.WriteLine(" Failed to resolve placeholders in body");
                return false;
            }

            // Resolve placeholders in headers
            Dictionary<string, string> resolvedHeaders = null;
            if (configRequest.Headers != null && configRequest.Headers.Count > 0)
            {
                resolvedHeaders = new Dictionary<string, string>();
                foreach (var header in configRequest.Headers)
                {
                    string resolvedValue = await _resolver.ResolvePlaceholderValue(header.Value, configRequest.Endpoint);
                    if (resolvedValue == null)
                    {
                        Console.WriteLine($" Failed to resolve placeholder in header: {header.Key}");
                        return false;
                    }
                    resolvedHeaders[header.Key] = resolvedValue;
                }
            }

            Console.WriteLine($" Placeholders resolved");

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
                    Console.WriteLine($"\n Executing activity: {activity}");
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
                    validation.IsSuccess,
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
                Console.WriteLine($" ERROR: {ex.Message}");
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
                request.Content = new StringContent(requestJson, Encoding.UTF8, "text/plain");

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
                Console.WriteLine(ex.ToString());
                throw;
            }

        }


    }
}
//90470544
//