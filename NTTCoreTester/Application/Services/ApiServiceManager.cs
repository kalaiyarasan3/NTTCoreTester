using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTTCoreTester.Application.Repositories;
using NTTCoreTester.Application.Shared.Models;
using NTTCoreTester.BusinessLogic;
using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Models.Common;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NTTCoreTester.Application.Services;

public class ApiServiceManager : IApiServiceManager
{
    private readonly HttpClient _httpClient;
    private readonly IAuthManager _authManager;
    private readonly ApiConfiguration _config;
    private readonly ILogger<ApiServiceManager> _logger;

    public ApiServiceManager(
        HttpClient httpClient,
        IAuthManager authManager,
        ILogger<ApiServiceManager> logger,
        ApiConfiguration config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        foreach (var header in _config.DefaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public async Task<ApiResult> PostAsyncRaw(
        string endpoint,
        string requestJson,
        IReadOnlyDictionary<string, string>? extraHeaders = null,
        CancellationToken ct = default)
    {
        try
        {
            var url = _config.BaseUrl + endpoint;

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("AuthToken", "/kzzcK9tsZeGYto65JH53xANQ=");

            if (extraHeaders != null)
            {
                foreach (var kv in extraHeaders)
                    requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            _logger.LogInformation("POST (RAW) {Endpoint}", endpoint);

            var response = await _httpClient.SendAsync(requestMessage, ct);
            var rawJson = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}\n{rawJson}");

            
        

         
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement.Clone();

            int statusCode = root.TryGetProperty("StatusCode", out var sc)
            ? sc.GetInt32()
            : 0;

            JsonElement? responseData = null;
            if (root.TryGetProperty("ResponceDataObject", out var rdo))
            {
                responseData = rdo.Clone();
            }

            return new ApiResult
            {
                StatusCode = statusCode,
                Root = root,
                ResponseData = responseData,
                RawJson = rawJson
            };


        }
        catch (Exception ex)
        {
            _logger.LogWarning("Got exception {ex}", ex);
            throw;
        }
    }


    public async Task<TResponse> PostAsync<TRequest, TResponse>(
     string endpoint,
     TRequest requestBody,
     IReadOnlyDictionary<string, string>? extraHeaders = null,
     CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var url = _config.BaseUrl + endpoint;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(requestBody)
            };

            var token = "9gZEhj/d2zuSgL2ICS6F41R6FqNS0+8QlBUTt/ia6cPviWTNMnhaC/FAR4/Z+yLGkYEYeEJNiN2/H6xuw9/kzzcK9tsZeGYto65JH53xANQ=";

            _logger.LogInformation("POST {Endpoint}", endpoint);

            requestMessage.Headers.Add("AuthToken", token);

            if (extraHeaders != null)
            {
                foreach (var kv in extraHeaders)
                {
                    requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
            stopwatch.Restart();
            var response = await _httpClient.SendAsync(requestMessage, ct);
            var rawContent = await response.Content.ReadAsStringAsync(ct);

            stopwatch.Stop();
            long ms = stopwatch.ElapsedMilliseconds;

            if (ms > 100)
            {
                _logger.LogWarning(
                    "[SLOW] {Endpoint} took {Ms} ms (HTTP {Code})",
                    endpoint, ms, (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
            }

            var apiResponse = JsonConvert
                .DeserializeObject<ApiResponse<TResponse>>(rawContent);

            if (apiResponse == null)
            {
                _logger.LogError("Failed to deserialize response");
                return default;
            }

            if (!string.Equals(apiResponse.Status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError($"Business Status not OK → {apiResponse.Status}");
                return default;
            }

            if (apiResponse.ResponceDataObject == null)
            {
                _logger.LogError("ResponceDataObject is null");
                return default;
            }
            return apiResponse.ResponceDataObject;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            long ms = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex,
                "[ERROR] {Endpoint} failed after {Ms} ms",
                endpoint, ms);

            throw;
        }
    }


}