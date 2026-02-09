using Microsoft.Extensions.Logging;
using NTTCoreTester.Application.Features.Orders.Request;
using NTTCoreTester.Application.Features.Orders.ViewModels;
using NTTCoreTester.Application.Helper;
using NTTCoreTester.Application.Services;
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

        public async Task<OrderInfoResponse> GetLastOrderStatusAsync(LastOrderRequest request)
        {
            const string activity = "GetLastOrderStatus";

            var extraHeaders = new Dictionary<string, string>
            {
                ["Module"] = "OrderService",
                ["Source"] = "RMS"
            };
            var response = await _apiManager
                .PostAsync<LastOrderRequest, OrderInfoResponse>(
                    activity,
                    request,
                    extraHeaders);

            return response ?? throw new NullReferenceException("API response is null");
        }
        public async Task<OrderMarginResponse> GetOrderMarginAsync(OrderMarginRequest request)
        {
            const string activity = "GetOrderMargin";

            var extraHeaders = new Dictionary<string, string>
            {
                ["Module"] = "OrderService",
                ["Source"] = "RMS"
            };

            var response = await _apiManager.PostAsync<OrderMarginRequest, OrderMarginResponse>(activity, request, extraHeaders);
            return response ?? throw new NullReferenceException("API response is null");
        }

        public async Task<SecurityInfoResponse> GetSecurityInfoAsync(GetSecurityInfoRequest request)
        {
            const string activity = "GetSecurityInfo";

            var extraHeaders = new Dictionary<string, string>
            {
                ["Module"] = "OrderService",
                ["Source"] = "RMS"
            };

            var response = await _apiManager.PostAsync<GetSecurityInfoRequest, SecurityInfoResponse>(activity, request, extraHeaders);
            return response ?? throw new NullReferenceException("API response is null");
        }

        public Task<PlaceOrderResponse> PlaceOrderAsync()
            => ExecuteJsonApiAsync<PlaceOrderResponse>(
                apiName: "PlaceOrder",
                activity: "PlaceOrder"
            );

        public Task<ModifyOrderResponse> ModifyOrderAsync()
            => ExecuteJsonApiAsync<ModifyOrderResponse>(
                apiName: "ModifyOrder",
                activity: "ModifyOrder"
            );

        public Task<CancelOrderResponse> CancelOrderAsync()
            => ExecuteJsonApiAsync<CancelOrderResponse>(
                apiName: "CancelOrder",
                activity: "CancelOrder"
            );

        public Task<SingleOrdHistResponse> SingleOrdHistAsync()
            => ExecuteJsonApiAsync<SingleOrdHistResponse>(
                apiName: "SingleOrdHist",
                activity: "SingleOrdHist"
            );


        private async Task<TResponse> ExecuteJsonApiAsync<TResponse>(
            string apiName,
            string activity,

            CancellationToken ct = default)
        {
            _logger.LogInformation("Executing JSON API: {ApiName}", apiName);

            var apiNode = JsonStore.GetApi(apiName);
             
            var rawRequestJson = apiNode.GetProperty("Request").GetRawText();
            var resolvedRequestJson = _variableManager.ReplaceVariables(rawRequestJson);

            var requestBody =
                JsonSerializer.Deserialize<Dictionary<string, object>>(
                    resolvedRequestJson
                )!;
             
            var headersNode = apiNode.GetProperty("Headers");
            var headers = new Dictionary<string, string>
            {
                ["Module"] = headersNode.GetProperty("Module").GetString()!,
                ["Source"] = headersNode.GetProperty("Source").GetString()!
            };
             
            return await _apiManager.PostAsync<
                Dictionary<string, object>,
                TResponse
            >(activity, requestBody, headers, ct);
        }


    }
}
