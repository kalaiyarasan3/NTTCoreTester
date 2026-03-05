using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTTCoreTester.Activities
{
    public class ValidateFundTransferFailureReason(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ValidateFundTransferFailureReason);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                bool isTransferred = cache.Get<bool>(Constants.IsTransferred);

                if (isTransferred)
                {
                    "Transfer succeeded. Skipping failure diagnostics.".Warn();
                    return ActivityResult.Success();
                }

                var preLimits = cache.Get<List<LimitMarginDetails>>(Constants.PreLimitMargin);

                if (preLimits == null)
                    return "PreLimits missing".FailWithLog(true);

                var mtf = preLimits
                    .FirstOrDefault(x => x.TemplateId == 2);

                if (mtf == null)
                    return "MTF margin block not found".FailWithLog(true);

                decimal transferable = mtf.TransferableAmount;
                decimal remaining = mtf.RemainingMargin;
                decimal used = mtf.UsedMarginWithoutPL;

                if (transferable <= 0)
                {
                    return $"Transfer rejected correctly. TransferableAmount is {transferable}."
                        .FailWithLog(false);
                }

                if (used > 0)
                {
                    return $"Transfer rejected because margin is already used. UsedMarginWithoutPL = {used}"
                        .FailWithLog(false);
                }

                var message = $"Transfer failed despite transferable margin available. " +
                       $"TransferableAmount = {transferable}, RemainingMargin = {remaining}";

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