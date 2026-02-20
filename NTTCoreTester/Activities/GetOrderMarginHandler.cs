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

        public string Name => "ExtractGetOrderMargin";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var dataObject = result.DataObject;

                if (dataObject == null)
                    return "GetOrderMargin DataObject is null".FailWithLog();

                var margin = dataObject.ToObject<OrderMarginDetails>();

                if (margin == null)
                    return "Failed to parse OrderMargin".FailWithLog();

                string? ordermargin = dataObject["ordermargin"]?.Value<string>();
                _cache.Set("ordermargin", ordermargin);

                _cache.Set(Constants.GetOrderMargin, margin);

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error in GetOrderMarginHandler: {ex.Message}".FailWithLog();
            }
        }
    }

}
