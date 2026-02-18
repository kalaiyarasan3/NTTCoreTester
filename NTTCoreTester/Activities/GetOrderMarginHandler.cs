using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;


namespace NTTCoreTester.Activities
{

    public class GetOrderMarginHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "GetOrderMargin";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.DataObject;

                if (dataObject == null)
                    return ActivityResult.HardFail("GetOrderMargin DataObject is null");

                var margin = dataObject.ToObject<OrderMarginDetails>();

                if (margin == null)
                    return ActivityResult.HardFail("Failed to parse OrderMargin");

                _cache.Set(Constants.GetOrderMargin, new OrderMarginDetails
                {
                    Charges = margin.Charges,
                    OrderMargin = margin.OrderMargin,
                    MarginUsedPrev = margin.MarginUsedPrev,
                    AvailableMargin = margin.AvailableMargin
                });

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
