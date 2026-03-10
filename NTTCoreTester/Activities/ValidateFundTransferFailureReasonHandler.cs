using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTTCoreTester.Activities
{
    public class ValidateFundTransferFailureReasonHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ValidateFundTransferFailureReasonHandler);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                bool isTransferred = cache.Get<bool>(Constants.IsTransferred);

                if (isTransferred)
                {
                    "Transfer succeeded. Skipping failure diagnostics.".Warn();
                    return ActivityResult.Success("Transfer succeeded. Skipping failure diagnostics.");
                }

                var preLimits = cache.Get<List<LimitMarginDetails>>(Constants.PreLimitMargin);

                if (preLimits == null)
                    return "PreLimits missing".FailWithLog(true);

                string direction = cache.Get<string>(Constants.TransferDirection);

                LimitMarginDetails? source = null;

                if (direction == "MTF_TO_NON_MTF")
                    source = preLimits.FirstOrDefault(x => x.TemplateId == 2);

                else if (direction == "NON_MTF_TO_MTF")
                    source = preLimits.FirstOrDefault(x => x.TemplateId == 1);

                if (source == null)
                    return $"Source margin bucket not found for direction {direction}".FailWithLog(true);

                decimal transferable = source.TransferableAmount;
                decimal remaining = source.RemainingMargin;
                decimal used = source.UsedMarginWithoutPL;

                if (transferable <= 0)
                {
                    return $"Transfer rejected correctly. TransferableAmount is {transferable}."
                        .FailWithLog(false);
                }

                var message =
                   $"Transfer failed despite transferable margin available. " +
                    $"Direction={direction}, TransferableAmount={transferable}, RemainingMargin={remaining}";
                
                return message.FailWithLog(true);
            }
            catch (Exception ex)
            {
                return $"Error in ValidateFundTransferFailureReasonHandler: {ex.Message}"
                    .FailWithLog(true);
            }
        }
    }
}