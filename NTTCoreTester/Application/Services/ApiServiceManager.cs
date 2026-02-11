using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTTCoreTester.Application.Repositories;
using NTTCoreTester.Core;  // ← Changed: Use Core instead of BusinessLogic
using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace NTTCoreTester.Application.Services;

public class ApiServiceManager : IApiServiceManager
{
    private readonly HttpClient _httpClient;
    private readonly ISessionManager _sessionManager;  // ← Changed: Use ISessionManager
    private readonly ApiConfiguration _config;
    private readonly ILogger<ApiServiceManager> _logger;

    public ApiServiceManager(
        HttpClient httpClient,
        ISessionManager sessionManager,  // ← Changed
        ILogger<ApiServiceManager> logger,
        ApiConfiguration config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));  // ← Changed
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        foreach (var header in _config.DefaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
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
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(requestBody)
            };

            var token = _sessionManager.GetToken();  // ← Changed

            _logger.LogInformation("POST {Endpoint}", endpoint);

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Add("AuthToken", token);
            }

            if (extraHeaders != null)
            {
                foreach (var kv in extraHeaders)
                {
                    requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

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
                throw new InvalidOperationException("Failed to deserialize response");

            if (!string.Equals(apiResponse.Status, "OK", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Business Status not OK → {apiResponse.Status}");

            if (apiResponse.ResponceDataObject == null)
                throw new InvalidOperationException(
                    "ResponceDataObject is null");

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
