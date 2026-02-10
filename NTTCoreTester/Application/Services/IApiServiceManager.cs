
using NTTCoreTester.Application.Shared.Models;

namespace NTTCoreTester.Application.Services;

public interface IApiServiceManager
{
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest requestBody, IReadOnlyDictionary<string, string>? extraHeaders = null, CancellationToken ct = default);
    Task<ApiResult> PostAsyncRaw(string endpoint, string requestJson, IReadOnlyDictionary<string, string>? extraHeaders = null, CancellationToken ct = default);
}