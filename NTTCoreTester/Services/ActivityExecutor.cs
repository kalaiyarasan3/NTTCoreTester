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
            if (string.IsNullOrEmpty(methodName))
                return false;

            try
            {
                var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null)
                {
                    Console.WriteLine($" Activity Method is not{methodName}");

                    return false;
                }

                //Invoke the method

                var result = method.Invoke(this, new Object[] { response, endpoint });

                return (bool)result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error executing activity method {methodName}:{ex.Message}");
                return false;
            }
        }
        private bool ExtractSession(string response, string endpoint)
        {
            try
            {
                var json = JObject.Parse(response);
                var dataObject = json["ResponceDataObject"];

                if (dataObject == null)
                {
                    Console.WriteLine("   ResponceDataObject not found in response");
                    return false;
                }

                string token = dataObject["susertoken"]?.Value<string>();
                string userId = dataObject["uid"]?.Value<string>();
                string userName = dataObject["uname"]?.Value<string>();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("   Session data incomplete");
                    return false;
                }

                _cache.Set("token", token);
                _cache.Set("userId", userId);
                _cache.Set("userName", userName);

                Console.WriteLine($"   Session saved: {userName} ({userId})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ExtractSession failed: {ex.Message}");
                return false;
            }
        }


        private bool GetLastOrderStatus(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.Json["ResponceDataObject"];

                if (dataObject == null)
                    return false;

                var orders = dataObject["AllOrders"]
                    .ToObject<List<OrderDetails>>();

                if (orders == null || !orders.Any())
                    return false;

                var key = _cache.Get(Constans.ClientOrdId);

                var lastOrder = orders.FirstOrDefault(x => x.cl_ord_id == key);

                if (lastOrder == null)
                    return false;

                _cache.Set(Constans.OrderNumber, lastOrder.ordno);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in GetLastOrderStatus: {ex.Message}");
                return false;
            }
        }


        private bool SendOtp(string response, string endpoint)
        {
            // Extract token from login response and store in cache
            Console.WriteLine(" Executing Login activity...");
            return true;
        }

        private bool Login(string response, string endpoint)
        {
            // Extract token from login response and store in cache
            Console.WriteLine($" Executing {endpoint} activity...");
            return true;
        }



        private bool ExtractClientOrdId(string response, string endponint)
        {

            var json = JObject.Parse(response);
            string cl_ord_id = json["ResponceDataObject"]?["cl_ord_id"]?.Value<string>();

            if (string.IsNullOrEmpty(cl_ord_id))
            {
                return false;
            }

            _cache.Set("cl_ord_id", cl_ord_id);


            return true;
        }
    }
 
    class LastOrderStatus
    {
        public List<OrderDetails> AllOrders { get; set; }
    }

    class OrderDetails
    {
        public string ordno { get; set; }
        public string cl_ord_id { get; set; }
    }

}

