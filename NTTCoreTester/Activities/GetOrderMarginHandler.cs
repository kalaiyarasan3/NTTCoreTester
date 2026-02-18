using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NTTCoreTester.Core; 


namespace NTTCoreTester.Activities
{
   
    public class GetOrderMarginHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public GetOrderMarginHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "GetOrderMargin";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.DataObject;

                if (dataObject == null)
                    return ActivityResult.HardFail("DataObject is null");

                var margin = dataObject.ToObject<OrderMarginDetails>();

                if (margin == null)
                    return ActivityResult.HardFail("Failed to parse OrderMargin");

                // Store in cache
                _cache.Set("AvailableMargin", margin.AvailableMargin.ToString());
                _cache.Set("OrderMargin", margin.OrderMargin.ToString());
                _cache.Set("MarginUsedPrev", margin.MarginUsedPrev.ToString());
                _cache.Set("Charges", margin.Charges.ToString());

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail(
                    $"Error in GetOrderMarginHandler: {ex.Message}");
            }
        }
    }

}
