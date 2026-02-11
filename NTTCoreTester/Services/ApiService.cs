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
        Task ExecuteRequest(string requestFileName);
        List<string> GetAvailableRequests();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ApiConfiguration _config;
        private readonly ResponseChecker _checker;
        private readonly ICsvReport _csvReport;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaceholderCache _cache;
        private const string REQUEST_FOLDER = "Requests";

        public ApiService(HttpClient http, ApiConfiguration config, ResponseChecker checker,
                         ICsvReport csvReport, ISessionManager sessionManager, IPlaceholderCache cache)
        {
            _http = http;
            _config = config;
            _checker = checker;
            _csvReport = csvReport;
            _sessionManager = sessionManager;
            _cache = cache;
            _http.BaseAddress = new Uri(_config.BaseUrl);

            if (!Directory.Exists(REQUEST_FOLDER))
                Directory.CreateDirectory(REQUEST_FOLDER);
        }

        public List<string> GetAvailableRequests()
        {
            if (!Directory.Exists(REQUEST_FOLDER))
                return new List<string>();

            var files = Directory.GetFiles(REQUEST_FOLDER, "*_request.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f).Replace("_request", "")).ToList();
        }

        public async Task ExecuteRequest(string requestFileName)
        {
            string filePath = Path.Combine(REQUEST_FOLDER, $"{requestFileName}_request.json");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"\n❌ Request file not found: {filePath}");
                return;
            }

            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine($"Executing: {requestFileName}");
            Console.WriteLine($"{new string('=', 80)}");

            string fileContent = await File.ReadAllTextAsync(filePath);
            Console.WriteLine($"📄 Template loaded from: {filePath}");

            // Check if file has headers section (structured format)
            Dictionary<string, string> customHeaders = null;
            string requestTemplate;

            try
            {
                var jsonObj = JObject.Parse(fileContent);

                if (jsonObj["headers"] != null && jsonObj["body"] != null)
                {
                    // Structured format with headers
                    customHeaders = jsonObj["headers"].ToObject<Dictionary<string, string>>();
                    requestTemplate = jsonObj["body"].ToString();
                    Console.WriteLine($"📋 Custom headers detected: {customHeaders.Count}");
                }
                else
                {
                    // Simple format (just body)
                    requestTemplate = fileContent;
                    Console.WriteLine($"📋 Using default headers from appsettings.json");
                }
            }
            catch
            {
                // If parsing fails, treat as simple body-only format
                requestTemplate = fileContent;
                Console.WriteLine($"📋 Using default headers from appsettings.json");
            }

            // Replace placeholders in body
            string requestJson = await ResolvePlaceholders(requestTemplate, requestFileName);
            if (requestJson == null)
            {
                Console.WriteLine("❌ Failed to resolve placeholders in body");
                return;
            }

            // Replace placeholders in custom headers
            if (customHeaders != null)
            {
                var resolvedHeaders = new Dictionary<string, string>();
                foreach (var header in customHeaders)
                {
                    string resolvedValue = await ResolvePlaceholderValue(header.Value, requestFileName);
                    if (resolvedValue == null)
                    {
                        Console.WriteLine($"❌ Failed to resolve placeholder in header: {header.Key}");
                        return;
                    }
                    resolvedHeaders[header.Key] = resolvedValue;
                }
                customHeaders = resolvedHeaders;
            }

            Console.WriteLine($"✅ Placeholders resolved");

            await CallApi(requestFileName, requestJson, customHeaders);
        }

        private async Task<string> ResolvePlaceholders(string template, string endpoint)
        {
            var matches = Regex.Matches(template, @"\{\{(\w+)\}\}");
            var placeholders = matches.Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();

            if (placeholders.Count == 0)
            {
                Console.WriteLine("ℹ️  No placeholders found in body");
                return template;
            }

            Console.WriteLine($"\n🔍 Found {placeholders.Count} placeholder(s) in body: {string.Join(", ", placeholders)}");

            string result = template;

            foreach (var placeholder in placeholders)
            {
                string value = await GetPlaceholderValue(placeholder, endpoint);
                if (value == null)
                {
                    Console.WriteLine($"❌ Failed to resolve: {placeholder}");
                    return null;
                }
                result = result.Replace($"{{{{{placeholder}}}}}", value);
            }

            return result;
        }

        private async Task<string> ResolvePlaceholderValue(string value, string endpoint)
        {
            // Check if value contains placeholder
            var match = Regex.Match(value, @"\{\{(\w+)\}\}");
            if (!match.Success)
                return value; // No placeholder, return as-is

            string placeholder = match.Groups[1].Value;
            string resolvedValue = await GetPlaceholderValue(placeholder, endpoint);

            if (resolvedValue == null)
                return null;

            return value.Replace($"{{{{{placeholder}}}}}", resolvedValue);
        }

        private async Task<string> GetPlaceholderValue(string placeholder, string endpoint)
        {
            // Session-based placeholders
            if (placeholder == "token")
            {
                string token = _sessionManager.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"❌ No active session. Please login first.");
                    return null;
                }
                Console.WriteLine($"   {{{{token}}}} → [from session]");
                return token;
            }

            if (placeholder == "userId")
            {
                string userId = _sessionManager.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"❌ No active session. Please login first.");
                    return null;
                }
                Console.WriteLine($"   {{{{userId}}}} → {userId} [from session]");
                return userId;
            }

            // Never-cached placeholders (always prompt)
            if (placeholder == "otp" || placeholder == "newPwd" || placeholder == "logintoken")
            {
                Console.Write($"   Enter {placeholder}: ");
                string value = placeholder == "newPwd" ? ReadPassword() : Console.ReadLine();
                return value;
            }

            // Cacheable placeholders
            if (_cache.Has(placeholder))
            {
                string cached = _cache.Get(placeholder);
                Console.WriteLine($"   {{{{{placeholder}}}}} → [cached]");
                return cached;
            }

            // Prompt and cache
            Console.Write($"   Enter {placeholder}: ");
            string input = placeholder == "pwd" ? ReadPassword() : Console.ReadLine();
            _cache.Set(placeholder, input);
            return input;
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private async Task CallApi(string endpoint, string requestJson, Dictionary<string, string> customHeaders)
        {
            var timer = Stopwatch.StartNew();

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

                // Override with custom headers from request file (if any)
                if (customHeaders != null && customHeaders.Count > 0)
                {
                    Console.WriteLine($"🔧 Applying {customHeaders.Count} custom header(s):");
                    foreach (var header in customHeaders)
                    {
                        Console.WriteLine($"   {header.Key}: {MaskSensitiveValue(header.Key, header.Value)}");
                        request.Headers.Remove(header.Key); // Remove default first
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                Console.WriteLine($"\n🌐 Calling API: {fullUrl}");

                var response = await _http.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($"⏱️  Response Time: {timer.ElapsedMilliseconds}ms");
                Console.WriteLine($"📡 HTTP Status: {(int)response.StatusCode}");

                // Validate schema
                bool schemaValid = _checker.Check(endpoint, respBody, out List<string> errors);
                string validationErrors = string.Join("; ", errors);

                // Extract business status
                string businessStatus = ExtractBusinessStatus(respBody);

                // Handle session for Login
                if (endpoint == "Login" && businessStatus == "SUCCESS")
                {
                    ExtractAndStoreSession(respBody);
                }

                // Handle session for Logout
                if (endpoint == "Logout")
                {
                    _sessionManager.ClearSession();
                    _cache.Clear();
                }

                // Log to CSV
                _csvReport.AddEntry(endpoint, timer.ElapsedMilliseconds, (int)response.StatusCode,
                                   businessStatus, respBody, schemaValid, validationErrors);

                // Display result
                Console.WriteLine($"💼 Business Status: {businessStatus}");
                Console.WriteLine($"📋 Schema Valid: {(schemaValid ? "YES" : "NO")}");

                if (!schemaValid)
                {
                    Console.WriteLine($"⚠️  Validation Errors: {validationErrors}");
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");

                _csvReport.AddEntry(endpoint, timer.ElapsedMilliseconds, 0, "ERROR",
                                   $"{{\"error\":\"{ex.Message}\"}}", false, $"Exception: {ex.Message}");
            }
        }

        private string MaskSensitiveValue(string key, string value)
        {
            if (key.ToLower() == "authtoken" && value.Length > 10)
            {
                return value.Substring(0, 3) + "***" + value.Substring(value.Length - 3);
            }
            return value;
        }

        private string ExtractBusinessStatus(string responseJson)
        {
            try
            {
                var json = JObject.Parse(responseJson);
                int statusCode = json["StatusCode"]?.Value<int>() ?? -1;
                return statusCode == 0 ? "SUCCESS" : "FAILED";
            }
            catch
            {
                return "UNKNOWN";
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
                        _sessionManager.SetSession(token, userId, userName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not extract session: {ex.Message}");
            }
        }
    }
}
