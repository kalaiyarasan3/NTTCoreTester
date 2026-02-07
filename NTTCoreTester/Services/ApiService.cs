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
            SetupHttpClient();
        }

        private void SetupHttpClient()
        {
            _http.BaseAddress = new Uri(_config.BaseUrl);

            foreach (var h in _config.DefaultHeaders)
            {
                _http.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
            }
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> SendOtp(SendOtpRequest req)
        {
            return await PostRequest<GeneralData>("?Activity=SendOTP", req);
        }

        public async Task<(ApiResponse<LoginData> res, long time, int status)> Login(LoginRequest req)
        {
            return await PostRequest<LoginData>("?Activity=Login", req);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> CheckLogin(CheckLoginRequest req)
        {
            return await PostRequest<GeneralData>("?Activity=CheckLogin", req);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> Logout(string token, string userId)
        {
            string url = $"?Activity=Logout&logintoken={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(userId)}";
            return await GetRequest<GeneralData>(url);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> ForgotPassword(ForgotPwdRequest req)
        {
            return await PostRequest<GeneralData>("?Activity=FgtPwdOTP", req);
        }

        public async Task<(ApiResponse<GeneralData> res, long time, int status)> ResetPassword(ResetPwdRequest req)
        {
            return await PostRequest<GeneralData>("?Activity=ValOTPStPwd", req);
        }

        // generic POST handler
        private async Task<(ApiResponse<T> res, long time, int status)> PostRequest<T>(string endpoint, object data)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "text/plain");

                Console.WriteLine($"\n>> POST {endpoint}");
                Console.WriteLine($">> Payload: {json}");  // Show actual data

                var response = await _http.PostAsync(endpoint, content);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($"<< {(int)response.StatusCode} ({timer.ElapsedMilliseconds}ms)");
                Console.WriteLine($"<< Response: {respBody}"); // Show actual response

                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(respBody);
                return (result, timer.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"!! Error: {ex.Message}");
                return (new ApiResponse<T> { Status = "Error", Message = ex.Message },
                        timer.ElapsedMilliseconds, 0);
            }
        }

        // generic GET handler
        private async Task<(ApiResponse<T> res, long time, int status)> GetRequest<T>(string endpoint)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"\n>> GET {endpoint}");

                var response = await _http.GetAsync(endpoint);
                string respBody = await response.Content.ReadAsStringAsync();
                timer.Stop();

                Console.WriteLine($"<< {(int)response.StatusCode} ({timer.ElapsedMilliseconds}ms)");

                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(respBody);
                return (result, timer.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine($"!! Error: {ex.Message}");
                return (new ApiResponse<T> { Status = "Error", Message = ex.Message },
                        timer.ElapsedMilliseconds, 0);
            }
        }

       
    }
}
