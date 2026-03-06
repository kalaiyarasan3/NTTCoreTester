using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractClientOrdId : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractClientOrdId(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => nameof(ExtractClientOrdId);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var clOrdId = result.DataObject?["cl_ord_id"]?.Value<string>();
            var requestTimeRaw = result.DataObject?["request_time"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(clOrdId))
                return $"cl_ord_id not found in response for endpoint {endpoint}".FailWithLog();

            _cache.Set(Constants.ClientOrdId, clOrdId);
            _cache.Set(Constants.PlaceOrderTime, requestTimeRaw);

            var log = $"Client order id: {clOrdId} | PlaceOrderTime = {requestTimeRaw}, Set block margin false";
            log.Warn();

            _cache.Set(Constants.ShouldBlockMargin, false);

            return ActivityResult.Success(log);
        }

    
    }

}
