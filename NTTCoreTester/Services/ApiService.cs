using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace NTTCoreTester.Services
{
    public interface IApiService
    {
        Task<(ApiResponse<GeneralData> res, long time, int status)> SendOtp(SendOtpRequest req);
        Task<(ApiResponse<LoginData> res, long time, int status)> Login(LoginRequest req);
        Task<(ApiResponse<GeneralData> res, long time, int status)> CheckLogin(CheckLoginRequest req);
        Task<(ApiResponse<GeneralData> res, long time, int status)> Logout(string token, string userId);
        Task<(ApiResponse<GeneralData> res, long time, int status)> ForgotPassword(ForgotPwdRequest req);
        Task<(ApiResponse<GeneralData> res, long time, int status)> ResetPassword(ResetPwdRequest req);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ApiConfiguration _config;

        public ApiService(HttpClient http, ApiConfiguration config)
        {
            _http = http;
            _config = config;
            _http.BaseAddress = new Uri(_config.BaseUrl);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> SendOtp(SendOtpRequest req)
        {
            return await PostRequest<GeneralData>("SendOTP", req);
        }

        public async Task<(ApiResponse<LoginData> res, long time, int status)> Login(LoginRequest req)
        {
            return await PostRequest<LoginData>("Login", req);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> CheckLogin(CheckLoginRequest req)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}CheckLogin";
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);

                string json = JsonConvert.SerializeObject(req);  //  creating {"sessionkey":"..."}
                request.Content = new StringContent(json, Encoding.UTF8, "text/plain");

                foreach (var header in _config.DefaultHeaders)
                {
                    string lowerKey = header.Key.ToLower();

                    if (lowerKey == "authtoken" ||
                        lowerKey == "module" ||
                        lowerKey == "content-type" ||
                        lowerKey == "content-length" ||
                        lowerKey == "host")
                        continue;

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Override with CheckLogin-specific headers
                request.Headers.TryAddWithoutValidation("AuthToken", req.sessionkey);  
                request.Headers.TryAddWithoutValidation("Module", "OrderService");     

                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine($"[REQUEST] POST {fullUrl}");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[HEADERS]");
                foreach (var header in request.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[REQUEST BODY]");
                Console.WriteLine(json);
                Console.WriteLine(new string('=', 80));

                var response = await _http.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($"\n[RESPONSE] Status: {(int)response.StatusCode}");
                Console.WriteLine($"[RESPONSE] Time: {timer.ElapsedMilliseconds}ms");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[RESPONSE BODY]");
                Console.WriteLine(respBody);
                Console.WriteLine(new string('=', 80) + "\n");

                var result = JsonConvert.DeserializeObject<ApiResponse<GeneralData>>(respBody);
                return (result ?? new ApiResponse<GeneralData>(), timer.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                return (new ApiResponse<GeneralData> { Status = "Error", Message = ex.Message },
                        timer.ElapsedMilliseconds, 0);
            }
        }



        public async Task<(ApiResponse<GeneralData> res, long time, int status)> Logout(string token, string userId)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}Logout";

                // Logout is POST, not GET!
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);

                // Small body (11 bytes - maybe empty JSON or user info)
                string json = JsonConvert.SerializeObject(new { uid = userId });
                request.Content = new StringContent(json, Encoding.UTF8, "text/plain");

                // Add default headers from config
                foreach (var header in _config.DefaultHeaders)
                {
                    string lowerKey = header.Key.ToLower();

                    if (lowerKey == "authtoken" ||
                        lowerKey == "content-type" ||
                        lowerKey == "content-length" ||
                        lowerKey == "host")
                        continue;

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                request.Headers.TryAddWithoutValidation("AuthToken", token);

                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine($"[REQUEST] POST {fullUrl}");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[HEADERS]");
                foreach (var header in request.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[REQUEST BODY]");
                Console.WriteLine(json);
                Console.WriteLine(new string('=', 80));

                var response = await _http.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($"\n[RESPONSE] Status: {(int)response.StatusCode}");
                Console.WriteLine($"[RESPONSE] Time: {timer.ElapsedMilliseconds}ms");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[RESPONSE BODY]");
                Console.WriteLine(respBody);
                Console.WriteLine(new string('=', 80) + "\n");

                var result = JsonConvert.DeserializeObject<ApiResponse<GeneralData>>(respBody);
                return (result ?? new ApiResponse<GeneralData>(), timer.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                return (new ApiResponse<GeneralData> { Status = "Error", Message = ex.Message },
                        timer.ElapsedMilliseconds, 0);
            }
        }


        public async Task<(ApiResponse<GeneralData> res, long time, int status)> ForgotPassword(ForgotPwdRequest req)
        {
            return await PostRequest<GeneralData>("FgtPwdOTP", req);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> ResetPassword(ResetPwdRequest req)
        {
            return await PostRequest<GeneralData>("ValOTPStPwd", req);
        }

        

        private async Task<(ApiResponse<T> res, long time, int status)> PostRequest<T>(string endpoint, object data)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                string json = JsonConvert.SerializeObject(data);

                string fullUrl = $"{_config.BaseUrl.TrimEnd('/')}{endpoint}";

                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
                request.Content = new StringContent(json, Encoding.UTF8, "text/plain");

                // Add headers
                foreach (var header in _config.DefaultHeaders)
                {
                    string lowerKey = header.Key.ToLower();

                    if (lowerKey == "content-type" ||
                        lowerKey == "content-length" ||
                        lowerKey == "host")
                        continue;

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Detailed logging
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine($"[REQUEST] POST {fullUrl}");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[HEADERS]");
                foreach (var header in request.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[REQUEST BODY]");
                Console.WriteLine(json);
                Console.WriteLine(new string('=', 80));

                var response = await _http.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                // Response logging
                Console.WriteLine($"\n[RESPONSE] Status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"[RESPONSE] Time: {timer.ElapsedMilliseconds}ms");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("[RESPONSE BODY]");
                Console.WriteLine(respBody);
                Console.WriteLine(new string('=', 80) + "\n");

                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(respBody);
                return (result ?? new ApiResponse<T>(), timer.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.WriteLine($"\nStack: {ex.StackTrace}");
                Console.WriteLine(new string('=', 80) + "\n");
                return (new ApiResponse<T> { Status = "Error", Message = ex.Message },
                        timer.ElapsedMilliseconds, 0);
            }
        }

       
    }
}
