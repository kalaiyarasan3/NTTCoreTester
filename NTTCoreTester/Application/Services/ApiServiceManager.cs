using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTTCoreTester.Application.Repositories;
using NTTCoreTester.BusinessLogic;
using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System.Diagnostics;
using System.Net.Http.Json;

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


    public async Task<TResponse> PostAsync<TRequest, TResponse>(
     string endpoint,
     TRequest requestBody,
     IReadOnlyDictionary<string, string>? extraHeaders = null,
     CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
           var url =  _config.BaseUrl+= endpoint;
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