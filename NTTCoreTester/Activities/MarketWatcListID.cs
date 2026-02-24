using NTTCoreTester.Core;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    internal class MarketWatcListID : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public MarketWatcListID(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "MarketWatcListID";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "DataObject is null".FailWithLog();

            string? MarketWatchId = dataObject["MarketWatchId"]?.ToString();
            _cache.Set("MarketWatchId", MarketWatchId);
     
            return ActivityResult.Success("Security info extracted and stored in cache");
        }

      
    }
}
