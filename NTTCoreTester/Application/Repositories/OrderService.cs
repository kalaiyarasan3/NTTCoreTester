using Microsoft.Extensions.Logging; 
using NTTCoreTester.Application.Helper;
using NTTCoreTester.Application.Services;
using NTTCoreTester.Application.Shared.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Repositories
{
    public class OrderService(IApiServiceManager apiManager,
        ILogger<OrderService> logger,
        VariableManager variableManager) : IOrderService
    {
        private readonly IApiServiceManager _apiManager = apiManager;
        private readonly ILogger<OrderService> _logger = logger;
        private readonly VariableManager _variableManager = variableManager;

        public Task<ApiResult> GetLastOrderStatusAsync() => ExecuteJsonApiAsync(apiName: "GetLastOrderStatus", activity: "GetLastOrderStatus");

        public Task<ApiResult> GetOrderMarginAsync() => ExecuteJsonApiAsync(apiName: "GetOrderMargin", activity: "GetOrderMargin");

        public Task<ApiResult> GetSecurityInfoAsync() => ExecuteJsonApiAsync(apiName: "GetSecurityInfo", activity: "GetSecurityInfo");
        public Task<ApiResult> GetUserInfoAsync() => ExecuteJsonApiAsync(apiName: "GetUserInfo", activity: "GetUserInfo");

        public Task<ApiResult> PlaceOrderAsync() => ExecuteJsonApiAsync("PlaceOrder", "PlaceOrder");

        public Task<ApiResult> ModifyOrderAsync() => ExecuteJsonApiAsync("ModifyOrder", "ModifyOrder");

        public Task<ApiResult> CancelOrderAsync() => ExecuteJsonApiAsync("CancelOrder", "CancelOrder");

        private async Task<ApiResult> ExecuteJsonApiAsync(
            string apiName,
            string activity,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Executing RAW JSON API: {ApiName}", apiName);

            var apiNode = JsonStore.GetApi(apiName);

            var rawRequestJson = apiNode.GetProperty("Request").GetRawText();
            var resolvedRequestJson = _variableManager.ReplaceVariables(rawRequestJson);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (apiNode.TryGetProperty("Headers", out var headersNode)
                && headersNode.ValueKind == JsonValueKind.Object)
            {
                foreach (var header in headersNode.EnumerateObject())
                {
                    headers[header.Name] =
                        _variableManager.ReplaceVariables(header.Value.GetString() ?? string.Empty);
                }
            }


            return await _apiManager.PostAsyncRaw(
                activity,
                resolvedRequestJson,
                headers,
                ct
            );
        }

    }
}
