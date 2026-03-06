using Microsoft.Extensions.Logging;
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
                var request = JObject.Parse(result.Request);

                string fromProduct = request["FromProduct"]?.ToString();
                string toProduct = request["ToProduct"]?.ToString();

                string direction;

                if (fromProduct == "MTF")
                    direction = "MTF_TO_NON_MTF";
                else if (toProduct == "MTF")
                    direction = "NON_MTF_TO_MTF";
                else
                    direction = "UNKNOWN";
                var log = $"Fund transfer succeeded direction: {direction}"; 
                log.Warn();
                cache.Set(Constants.TransferDirection, direction);
                cache.Set(Constants.IsTransferred, true);

                return ActivityResult.Success(log);
            }
            catch (Exception ex)
            {
                return $"Error ExtractFundTransferStatusHandler: {ex.Message}"
                    .FailWithLog(true);
            }
        }
    }
}