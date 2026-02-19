using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractPreLimitMarginHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ExtractPreLimitMargin";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var margin = GetPrimaryLimitMargin(result);

                if (margin == null)
                    return ActivityResult.HardFail("Failed to parse LimitMarginDetails");
                 
                _cache.Set(Constants.PreLimitMargin, margin);                 

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail(
                    $"Error in ExtractPreLimitMarginHandler: {ex.Message}");
            }
        }

        private LimitMarginDetails? GetPrimaryLimitMargin(ApiExecutionResult result)
        {
            var marginsArray = result.DataObject?[Constants.AllMargins];

            if (marginsArray == null || marginsArray.Type != JTokenType.Array)
                return null;

            var selected = marginsArray
                .FirstOrDefault(x => x[Constants.TemplateId]?.Value<int>() == 1);

            return selected?.ToObject<LimitMarginDetails>();
        }
    }
}
