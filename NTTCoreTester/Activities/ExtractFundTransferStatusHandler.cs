using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractFundTransferStatusHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ExtractFundTransferStatusHandler);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                cache.Set(Constants.IsTransferred, true);

                $"Fund transfer succeeded".Warn();

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error ExtractFundTransferStatusHandler: {ex.Message}"
                    .FailWithLog(true);
            }
        }
    }
}