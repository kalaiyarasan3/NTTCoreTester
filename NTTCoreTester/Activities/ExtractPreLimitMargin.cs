using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractPreLimitMargin(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractPreLimitMargin);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var marginLimits = GetPrimaryLimitMargin(result);

                if (marginLimits == null)
                    return "Failed to parse LimitMarginDetails".FailWithLog();

                _cache.Set(Constants.IsTransferred, false);
                _cache.Set(Constants.PreLimitMargin, marginLimits);

                return ActivityResult.Success("Fethed Users Fund Details before placing order");
            }
            catch (Exception ex)
            {
                return $"Error in ExtractPreLimitMarginHandler: {ex.Message}".FailWithLog();
            }
        }

        private List<LimitMarginDetails>? GetPrimaryLimitMargin(ApiExecutionResult result)
        {
            var marginsArray = result.DataObject?[Constants.AllMargins];

            if (marginsArray == null || marginsArray.Type != JTokenType.Array)
                return null;             

            return marginsArray?.ToObject<List<LimitMarginDetails>>();
        }
    }
}
