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
        List<string> GetAvailableRequests();
        Task<bool> ExecuteRequestFromConfig(ConfigRequest configRequest);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ApiConfiguration _config;
        private readonly ResponseChecker _checker;
        private readonly CsvReport _csvReport;
        private readonly PlaceholderCache _cache;
        private const string REQUEST_FOLDER = "Requests";
        private readonly ActivityExecutor _activityExecutor;
        private readonly PlaceholderResolver _resolver;

        public ApiService(HttpClient http, ApiConfiguration config, ResponseChecker checker,
                         CsvReport csvReport, PlaceholderCache cache, ActivityExecutor activityExecutor, PlaceholderResolver resolver)
        {
            _http = http;
            _config = config;
            _checker = checker;
            _csvReport = csvReport;
            _cache = cache;
            _http.BaseAddress = new Uri(_config.BaseUrl);

            if (!Directory.Exists(REQUEST_FOLDER))
                Directory.CreateDirectory(REQUEST_FOLDER);
            _activityExecutor = activityExecutor;
            _resolver = resolver;
        }

        public List<string> GetAvailableRequests()
        {
            if (!Directory.Exists(REQUEST_FOLDER))
                return new List<string>();

            var files = Directory.GetFiles(REQUEST_FOLDER, "*_request.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f).Replace("_request", "")).ToList();
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

            return await CallApi(configRequest.Endpoint, requestJson, resolvedHeaders,configRequest.Activity);
        }

        private async Task<bool> CallApi(string endpoint, string requestJson, Dictionary<string, string> customHeaders, string activity = null)
        {
            var timer = Stopwatch.StartNew();
            bool success = false;

            try
            {
                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}{endpoint}";

                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
                request.Content = new StringContent(requestJson, Encoding.UTF8, "text/plain");

                // Start with default headers from config
                foreach (var header in _config.DefaultHeaders)
                {
                    string lowerKey = header.Key.ToLower();
                    if (lowerKey == "content-type" || lowerKey == "content-length" || lowerKey == "host")
                        continue;

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (customHeaders != null && customHeaders.Count > 0)
                {
                    Console.WriteLine($" Applying {customHeaders.Count} custom header(s):");
                    foreach (var header in customHeaders)
                    {
                        Console.WriteLine($"   {header.Key}: {header.Value}");
                        request.Headers.Remove(header.Key);
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                Console.WriteLine($"\n Calling API: {fullUrl}");

                var response = await _http.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($" Response Time: {timer.ElapsedMilliseconds}ms");
                Console.WriteLine($" HTTP Status: {(int)response.StatusCode}");

                bool schemaValid = _checker.Check(endpoint, respBody, out List<string> errors);
                string validationErrors = string.Join("; ", errors);

                string businessStatus = _checker.ExtractBusinessStatus(respBody);
                success = (businessStatus == "SUCCESS");

                if (endpoint == "Login" && businessStatus == "SUCCESS")
                {
                    ExtractAndStoreSession(respBody);
                }

                if (endpoint == "Logout")
                {
                    _cache.Clear();
                    Console.WriteLine("✓ Session cleared");
                }



                // Excute activity
                //---
                bool activitySuccess = true;
                if(!string.IsNullOrEmpty(activity))
                {
                    if(schemaValid&&businessStatus== "SUCCESS")
                    {
                        Console.WriteLine($"\n Executing activity: {activity}");
                        activitySuccess=_activityExecutor.Execute(activity, respBody, endpoint);
                        if (activitySuccess)
                            Console.WriteLine("Activity Executed Succssfully");
                        else
                            Console.WriteLine("Activity Execution Failed");
                    }
                }
                else {
                    Console.WriteLine($"skipping activity {activity} response not valid" );
                }
                //---


                _csvReport.AddEntry(endpoint, timer.ElapsedMilliseconds, (int)response.StatusCode,
                                   businessStatus, respBody, schemaValid, validationErrors);

                Console.WriteLine($" Business Status: {businessStatus}");
                Console.WriteLine($" Schema Valid: {(schemaValid ? "YES" : "NO")}");

                if (!schemaValid)
                {
                    Console.WriteLine($" Validation Errors: {validationErrors}");
                }

                return success && activitySuccess;
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"\n ERROR: {ex.Message}");

                _csvReport.AddEntry(endpoint, timer.ElapsedMilliseconds, 0, "ERROR",
                                   $"{{\"error\":\"{ex.Message}\"}}", false, $"Exception: {ex.Message}");

                return false;
            }
        }

        private void ExtractAndStoreSession(string responseJson)
        {
            try
            {
                var json = JObject.Parse(responseJson);
                var dataObject = json["ResponceDataObject"];

                if (dataObject != null)
                {
                    string token = dataObject["susertoken"]?.Value<string>();
                    string userId = dataObject["uid"]?.Value<string>();
                    string userName = dataObject["uname"]?.Value<string>();

                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                    {
                        _cache.Set("token", token);
                        _cache.Set("userId", userId);
                        _cache.Set("userName", userName);
                        Console.WriteLine($" Session saved: {userName} ({userId})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not extract session: {ex.Message}");
            }
        }

    }
}
