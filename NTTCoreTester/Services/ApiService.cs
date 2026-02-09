using Newtonsoft.Json;
using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Models.Auth;
using NTTCoreTester.Models.Common;
using NTTCoreTester.Services;
using System.Diagnostics;
using System.Text;

namespace NTTCoreTester.Services
{
    public interface IApiService
    {
        // All methods now return (string jsonResponse, long ms, int httpCode)
        Task<(string json, long ms, int httpCode)> SendOtp(SendOtpRequest req);
        Task<(string json, long ms, int httpCode)> Login(LoginRequest req);
        Task<(string json, long ms, int httpCode)> CheckLogin(CheckLoginRequest req);
        Task<(string json, long ms, int httpCode)> Logout(string token, string uid);
        Task<(string json, long ms, int httpCode)> ForgotPassword(ForgotPwdRequest req);
        Task<(string json, long ms, int httpCode)> ResetPassword(ResetPwdRequest req);
    }
}

public class ApiService : IApiService
{
    private readonly HttpClient _client;
    private readonly ApiConfiguration _cfg;

    public ApiService(HttpClient client, ApiConfiguration cfg)
    {
        _client = client;
        _cfg = cfg;

        // Setup headers
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", "*/*");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _client.DefaultRequestHeaders.Add("AuthToken", "DEFAULT");
        _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _client.DefaultRequestHeaders.Add("DeviceId", "MyAPI");
        _client.DefaultRequestHeaders.Add("Module", "DEFAULT");
        _client.DefaultRequestHeaders.Add("Source", "RMS");
        _client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.51.1");
    }

    public async Task<(string json, long ms, int httpCode)> SendOtp(SendOtpRequest req)
    {
        return await SendRequest("SendOTP", req);
    }

    public async Task<(string json, long ms, int httpCode)> Login(LoginRequest req)
    {
        return await SendRequest("Login", req);
    }

    public async Task<(string json, long ms, int httpCode)> CheckLogin(CheckLoginRequest req)
    {
        return await SendRequest("CheckLogin", req);
    }

    public async Task<(string json, long ms, int httpCode)> Logout(string token, string uid)
    {
        var req = new { sessionkey = token, uid = uid };
        return await SendRequest("Logout", req);
    }

    public async Task<(string json, long ms, int httpCode)> ForgotPassword(ForgotPwdRequest req)
    {
        return await SendRequest("FgtPwdOTP", req);
    }

    public async Task<(string json, long ms, int httpCode)> ResetPassword(ResetPwdRequest req)
    {
        return await SendRequest("ValOTPStPwd", req);
    }

    /// <summary>
    /// Generic method to send HTTP POST and return raw JSON
    /// </summary>
    private async Task<(string json, long ms, int httpCode)> SendRequest(string activity, object payload)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            string endpoint = $"{_cfg.BaseUrl}{activity}";
            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "text/plain");

            LogRequest(endpoint, jsonPayload);

            var response = await _client.PostAsync(endpoint, content);
            sw.Stop();

            int httpCode = (int)response.StatusCode;
            string responseBody = await response.Content.ReadAsStringAsync();

            LogResponse(response, sw.ElapsedMilliseconds, responseBody);

            return (responseBody, sw.ElapsedMilliseconds, httpCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"\n❌ Exception: {ex.Message}");
            return (null, sw.ElapsedMilliseconds, 0);
        }
    }

    private void LogRequest(string endpoint, string jsonPayload)
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine($"[REQUEST] POST {endpoint}");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("[HEADERS]");
        foreach (var header in _client.DefaultRequestHeaders)
        {
            Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("[REQUEST BODY]");
        Console.WriteLine(jsonPayload);
        Console.WriteLine(new string('=', 80));
    }

    private void LogResponse(HttpResponseMessage response, long elapsedMs, string body)
    {
        Console.WriteLine($"\n[RESPONSE] Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine($"[RESPONSE] Time: {elapsedMs}ms");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("[RESPONSE BODY]");
        Console.WriteLine(body);
        Console.WriteLine(new string('=', 80));
    }
}