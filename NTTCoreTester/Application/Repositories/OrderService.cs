using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NTTCoreTester.Application.Features.Orders.Request;
using NTTCoreTester.Application.Features.Orders.ViewModels;
using NTTCoreTester.Application.Services;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Repositories
{
    public class OrderService(IApiServiceManager apiManager, ILogger<OrderService> logger) : IOrderService
    {
        private readonly IApiServiceManager _apiManager = apiManager;
        private readonly ILogger<OrderService> _logger = logger;

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

    }
}
