using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Services
{
    public class ActivityExecutor : IActivityExecutor
    {
        private readonly IPlaceholderCache _cache;
        public ActivityExecutor(IPlaceholderCache cache) { _cache = cache; }

        public bool Execute(string methodName, string response, string endpoint)
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

                var result=method.Invoke(this,new Object[] { response, endpoint });

                return (bool)result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error executing activity method {methodName}: {ex.Message}");
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
            return ExtractToken(response, endpoint);
        }



        private bool ExtractToken(string response, string endponint)
        {

            var json = JObject.Parse(response);
            string ordno = json["ResponceDataObject"]?["ordno"]?.Value<string>();
            string cl_ord_id = json["ResponceDataObject"]?["cl_ord_id "]?.Value<string>();

            if(string.IsNullOrEmpty(ordno)|| string.IsNullOrEmpty(cl_ord_id))
            {
                return false;
            }

            _cache.Set("ordno", ordno);
                _cache.Set("cl_ord_id", cl_ord_id);


            return true;
        }
    }
}

