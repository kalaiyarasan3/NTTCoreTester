using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractClientOrdIdHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractClientOrdIdHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ExtractClientOrdId";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var clOrdId = result.DataObject?["cl_ord_id"]?.Value<string>();
            

            if (string.IsNullOrWhiteSpace(clOrdId))
                return ActivityResult.HardFail("cl_ord_id not found");

            _cache.Set(Constants.ClientOrdId, clOrdId);

            return ActivityResult.Success();
        }
    }

}
