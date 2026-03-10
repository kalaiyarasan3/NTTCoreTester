using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Services;

namespace NTTCoreTester.Activities
{
    public class LoginCheck : IActivityHandler
    {
        private readonly PlaceholderCache _cache;
        private readonly IApiService _apiService;

        public LoginCheck(PlaceholderCache cache, IApiService apiService)
        {
            _cache = cache;
            _apiService = apiService;
        }

        public string Name => "LoginCheck";

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            Console.WriteLine($"\n{new string('─', 60)}");
            Console.WriteLine($" LOGIN CHECK — acquiring tokens...");
            Console.WriteLine(new string('─', 60));

            var loginTokens = await GetLoginToken(endpoint, payLoad);

            if (loginTokens.Count == 0)
                return "No tokens acquired during login.".FailWithLog();

            Console.WriteLine($"\n Acquired {loginTokens.Count} token(s). Running CheckLogin...\n");

            var checkedResults = await CheckLogin(loginTokens);

            Console.WriteLine($"\n{new string('─', 60)}");
            Console.WriteLine($" SUMMARY: {checkedResults.Count}/{loginTokens.Count} tokens PASSED");
            Console.WriteLine(new string('─', 60));

            return checkedResults.Count > 0
                ? ActivityResult.Success($"{checkedResults.Count}/{loginTokens.Count} tokens passed CheckLogin.")
                : "All tokens failed CheckLogin.".FailWithLog();
        }

        private async Task<Dictionary<int, string>> GetLoginToken(string endpoint, string payLoad)
        {
            var loginToken = new Dictionary<int, string>();

            for (int i = 1; i <= 5; i++)
            {
                Console.Write($"  [{i:D2}] Login attempt... ");

                var headers = _apiService.ResolveHeaders("LoginHeaders");
                var response = await _apiService.SendRequest(endpoint, payLoad, headers);

                if (response?.StatusCode != 200)
                {
                    Console.WriteLine($"HTTP {response?.StatusCode} — stopping.");
                    break;
                }

                var json = JObject.Parse(response.ResponseBody);
                int statusCode = json["StatusCode"]?.Value<int>() ?? -1;

                if (statusCode > 0)
                {
                    var msg = json["Message"]?.Value<string>() ?? "Unknown error";
                    Console.WriteLine($"FAILED (StatusCode:{statusCode} — {msg})");
                    break;
                }

                var token = json["ResponceDataObject"]?["susertoken"]?.Value<string>();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    loginToken[i] = token;
                    Console.WriteLine($"OK — token acquired [{token}...]");
                }
                else
                {
                    Console.WriteLine("FAILED — susertoken missing in response.");
                }
            }

            return loginToken;
        }

        private async Task<Dictionary<int, string>> CheckLogin(Dictionary<int, string> logToken)
        {
            var result = new Dictionary<int, string>();

            if (logToken == null || logToken.Count == 0)
                return result;

            foreach (var (index, token) in logToken.OrderByDescending(x => x.Key))
            {
                Console.Write($"  [{index:D2}] CheckLogin for token [{token[..Math.Min(12, token.Length)]}...]... ");

                var header = _apiService.ResolveHeaders("AuthorizedHeaders", token);
                var payload = new JObject().ToString();
                var response = await _apiService.SendRequest("CheckLogin", payload, header);

                if (response?.StatusCode != 200)
                {
                    Console.WriteLine($"HTTP {response?.StatusCode} — stopping.");
                    break;
                }

                var json = JObject.Parse(response.ResponseBody);
                int statusCode = json["StatusCode"]?.Value<int>() ?? -1;
                var message = json["Message"]?.Value<string>();

                if (statusCode > 0)
                {
                    Console.WriteLine($"FAILED (StatusCode:{statusCode} — {message})");
                    break;
                }

                bool passed = message == "LoggedIn" && statusCode == 0;

                if (passed)
                {
                    result.Add(index, message!);
                    Console.WriteLine(" PASSED (LoggedIn)");
                }
                else
                {
                    Console.WriteLine($" FAILED (Message: {message ?? "null"})");
                }
            }

            return result;
        }
    }
}
