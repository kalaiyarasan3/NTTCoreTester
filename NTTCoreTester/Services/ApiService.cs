using Newtonsoft.Json;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Reporting;
using NTTCoreTester.Validators;
using System.Diagnostics;
using System.Text;

namespace NTTCoreTester.Services
{
    public interface IApiService
    {
        Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest, TestSuiteConfig testSuiteConfig);

        Dictionary<string, string> ResolveHeaders(string profileName, string token = "");

        Task<ApiExecutionResult> SendRequest(
            string endpoint,
            string requestJson,
            Dictionary<string, string> headers
        );
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
            _activityExecutor = activityExecutor;
            _cache = cache;
            _csvReport = csvReport;

            _http.BaseAddress = new Uri(_config.BaseUrl);
        }

        public async Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest, TestSuiteConfig testSuiteConfig)
        {
            try
            {
                $"\n{new string('=', 80)}".Debug();
                $"Executing Api: {configRequest.Endpoint}".Debug();
                $"{new string('=', 80)}".Debug();

                string requestJson = ResolvePayload(configRequest.Payload);

                string profileName = configRequest.HeaderProfileName ?? "AuthorizedHeaders";
                var headers = ResolveHeaders(profileName);

                return await CallApi(
                    configRequest.Endpoint,
                    requestJson,
                    headers,
                    configRequest.Activity,
                    configRequest.Description,
                    testSuiteConfig
                );
            }
            catch (Exception ex)
            {
                $" ERROR in Api service: {ex.Message}".Error();
                return false;
            }
        }

        public string ResolvePayload(object payload)
        {
            string requestJson = JsonConvert.SerializeObject(payload);

            var variableResult = _cache.ReplaceVariables(requestJson);

            if (!variableResult.IsSuccess)
                throw new Exception($"Failed to resolve placeholders: {variableResult.Error}");

            return variableResult.Text;
        }

        public Dictionary<string, string> ResolveHeaders(string profileName, string token = "")
        {
            Dictionary<string, string> resolvedHeaders = new();

            if (_config.HeaderProfiles == null) return resolvedHeaders;
            if (!_config.HeaderProfiles.TryGetValue(profileName, out var profileHeaders))
                return resolvedHeaders;

            foreach (var header in profileHeaders)
            {
                string value = header.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    if (header.Key.Equals("AuthToken", StringComparison.OrdinalIgnoreCase))
                        value = token; 
                }

                var headerVariable = _cache.ReplaceVariables(value);

                if (!headerVariable.IsSuccess)
                    throw new Exception($"Failed to resolve placeholder in header profile: {header.Key}");

                resolvedHeaders[header.Key] = headerVariable.Text;
            }

            return resolvedHeaders;
        }


        public async Task<bool> CallApi(
            string endpoint,
            string requestJson,
            Dictionary<string, string> headers,
            string activity,
            string description,
            TestSuiteConfig testSuiteconfig)
        {
            try
            {
                var result = await SendRequest(endpoint, requestJson, headers);

                var validation = _checker.Validate(result);

                ActivityResult activityResult = ActivityResult.Success();

                if (validation.IsSuccess && !string.IsNullOrEmpty(activity))
                {
                    $"\n Executing activity: {activity}".Debug();

                    result.Request = requestJson;

                    activityResult =await _activityExecutor.Execute(
                        activity,
                        result,
                        result.Endpoint,
                        requestJson
                    );
                }

                _csvReport.AddEntry(
                    testSuiteconfig.TestName,
                    result.Endpoint,
                    description,
                    string.IsNullOrWhiteSpace(activity) ? "No Activity" : activity,
                    activityResult.IsSuccess ? "Success" : "Failed",
                    result.ResponseTime,
                    result.StatusCode,
                    validation.BusinessStatus,
                    activityResult.Message,
                    result.ResponseBody,
                    validation.IsSchemaValid,
                    string.Join("; ", validation.Errors),
                    validation.Message
                );

                if (!validation.IsSuccess)
                    return false;

                if (!activityResult.IsSuccess && !activityResult.ContinueExecution)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                $" ERROR in CallApi: {ex.Message}".Error();
                return false;
            }
        }

        public async Task<ApiExecutionResult> SendRequest(
            string endpoint,
            string requestJson,
            Dictionary<string, string> headers)
        {
            try
            {
                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}{endpoint}";
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);

                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var timer = Stopwatch.StartNew();

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