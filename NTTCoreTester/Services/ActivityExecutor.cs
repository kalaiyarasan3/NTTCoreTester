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

        public bool Execute(string methodName, ApiExecutionResult response, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return false;

            try
            {
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null || method.ReturnType != typeof(bool))
                {
                    Console.WriteLine($" Activity Method {methodName} not found.");
                    return false;
                }

                var parameters = method.GetParameters();

                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(ApiExecutionResult) ||
                    parameters[1].ParameterType != typeof(string))
                {
                    return false;
                }                 

                var result = method.Invoke(this, new Object[] { response, endpoint });

                return (bool)result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error executing activity method {methodName}:{ex.Message}");
                return false;
            }
        }

        private bool ExtractSession(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return false;

            string? token = dataObject["susertoken"]?.Value<string>();
            string? userId = dataObject["uid"]?.Value<string>();
            string? userName = dataObject["uname"]?.Value<string>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return false;

            _cache.Set(Constants.SUserToken, token);
            _cache.Set(Constants.UId, userId);
            _cache.Set(Constants.UName, userName);

            return true;
        }

        private bool GetLastOrderStatus(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.DataObject;

                if (dataObject == null)
                {
                    Console.WriteLine("   ResponceDataObject not found.");
                    return false;
                }
                 
                var allOrdersToken = dataObject["AllOrders"];

                if (allOrdersToken == null || allOrdersToken.Type != JTokenType.Array)
                {
                    Console.WriteLine("   AllOrders not found or invalid.");
                    return false;
                }

                var orders = allOrdersToken.ToObject<List<OrderDetails>>();

                if (orders == null || !orders.Any())
                {
                    Console.WriteLine("   No orders returned.");
                    return false;
                }
                 
                var key = _cache.Get(Constants.ClientOrdId);

                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("   ClientOrdId not found in cache.");
                    return false;
                }
                 
                var lastOrder = orders.FirstOrDefault(x => x.ClientOrderId == key);

                if (lastOrder == null)
                {
                    Console.WriteLine("   Matching order not found.");
                    return false;
                }

                if(lastOrder.Status != "1111")
                {
                    Console.WriteLine($"{lastOrder.Remarks}.");
                    return false;
                }
                 
                _cache.Set(Constants.OrderNumber, lastOrder.OrderNumber);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error in GetLastOrderStatus: {ex.Message}");
                return false;
            }
        }


        private bool ExtractClientOrdId(ApiExecutionResult result, string endpoint)
        {
            string? cl_ord_id = result.DataObject?["cl_ord_id"]?.Value<string>();

            if (string.IsNullOrEmpty(cl_ord_id))
                return false;

            _cache.Set("cl_ord_id", cl_ord_id);
            return true;
        }

    }

   

}

