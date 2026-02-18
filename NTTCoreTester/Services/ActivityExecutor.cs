using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace NTTCoreTester.Services
{
    public class ActivityExecutor
    {
        private readonly PlaceholderCache _cache;
        public ActivityExecutor(PlaceholderCache cache) { _cache = cache; }

        public ActivityResult Execute(string methodName, ApiExecutionResult response, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return ActivityResult.Success();

            try
            {
                var method = GetType().GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null)
                {
                    Console.WriteLine($"Activity Method '{methodName}' not found.");
                    return ActivityResult.HardFail($"Activity '{methodName}' not found.");
                }

                if (method.ReturnType != typeof(ActivityResult))
                {
                    return ActivityResult.HardFail(
                        $"Activity '{methodName}' must return ActivityResult.");
                }

                var parameters = method.GetParameters();

                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(ApiExecutionResult) ||
                    parameters[1].ParameterType != typeof(string))
                {
                    return ActivityResult.HardFail(
                        $"Activity '{methodName}' has invalid signature.");
                }

                var result = method.Invoke(this, new object[] { response, endpoint });

                if (result is ActivityResult activityResult)
                    return activityResult;

                return ActivityResult.HardFail(
                    $"Activity '{methodName}' did not return ActivityResult.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing activity '{methodName}': {ex.Message}");
                return ActivityResult.HardFail(
                    $"Exception in activity '{methodName}': {ex.Message}");
            }
        }

        private ActivityResult ExtractOTP (ApiExecutionResult result, string endpoint)
        {
            Console.Write($"Enter Opt: ");
            string otp=Console.ReadLine();
            if (string.IsNullOrEmpty(otp))
                return ActivityResult.HardFail("OTP cannot be empty");
            _cache.Set("otp", otp);
            return ActivityResult.Success();
        }


        private ActivityResult ExtractSession(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return ActivityResult.HardFail("DataObject is null");

            string? token = dataObject["susertoken"]?.Value<string>();
            string? userId = dataObject["uid"]?.Value<string>();
            string? userName = dataObject["uname"]?.Value<string>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return ActivityResult.HardFail("Invalid login response");

            _cache.Set(Constants.SUserToken, token);
            _cache.Set(Constants.UId, userId);
            _cache.Set(Constants.UName, userName);

            return ActivityResult.Success();
        }


        private ActivityResult GetLastOrderStatus(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.DataObject;

                if (dataObject == null)
                    return ActivityResult.HardFail("ResponseDataObject not found");

                var allOrdersToken = dataObject["AllOrders"];

                if (allOrdersToken == null || allOrdersToken.Type != JTokenType.Array)
                    return ActivityResult.HardFail("AllOrders not found or invalid");

                var orders = allOrdersToken.ToObject<List<OrderDetails>>();

                if (orders == null || !orders.Any())
                    return ActivityResult.HardFail("No orders returned");

                var key = _cache.Get(Constants.ClientOrdId);

                if (string.IsNullOrEmpty(key))
                    return ActivityResult.HardFail("ClientOrdId not found in cache");

                var lastOrder = orders.FirstOrDefault(x => x.ClientOrderId == key);

                if (lastOrder == null)
                {
                    Console.WriteLine($"Matching order not found in {endpoint}");
                    return ActivityResult.HardFail("Order not found in normal status");
                }

                if (lastOrder.Status != "1111")
                {
                    Console.WriteLine(lastOrder.Remarks);
                    return ActivityResult.HardFail(lastOrder.Remarks ?? "Order rejected");
                }

                _cache.Set(Constants.OrderNumber, lastOrder.OrderNumber);

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error in GetLastOrderStatus: {ex.Message}");
            }
        }

        private ActivityResult ExtractClientOrdId(ApiExecutionResult result, string endpoint)
        {
            string? cl_ord_id = result.DataObject?["cl_ord_id"]?.Value<string>();

            if (string.IsNullOrEmpty(cl_ord_id))
                return ActivityResult.HardFail("cl_ord_id not found");

            _cache.Set(Constants.ClientOrdId, cl_ord_id);

            return ActivityResult.Success();
        }

        private ActivityResult ExtractMarketWatchId(ApiExecutionResult result, string endpoint)
        {
            string? wlid = result.DataObject?["MarketWatchId"]?.Value<string>();
            string? mw_name = result.DataObject?["MarketWatchName"]?.Value<string>();

            if (string.IsNullOrEmpty(wlid) && string.IsNullOrEmpty(mw_name))
                return ActivityResult.HardFail("MarketWatchId or MarketWatchName not found");

            _cache.Set("wlid", wlid);
            _cache.Set("MarketWatchName", mw_name);

            return ActivityResult.Success();
        }
    }
}

