using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractPostLimitMarginHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ExtractPostLimitMargin";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var errors = new List<string>();

                var postLimit = GetPrimaryLimitMargin(result);

                if (postLimit == null)
                    return "Post Limit margin not found".FailWithLog();

                var preLimit = _cache.Get<LimitMarginDetails>(Constants.PreLimitMargin);
                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog();

                if (orderMargin == null)
                    return "OrderMargin missing".FailWithLog();

                var debugMessage =
                $"preLimit.RemainingMargin: {preLimit.RemainingMargin}\n" +
                $"postLimit.RemainingMargin: {postLimit.RemainingMargin}\n" +
                $"preLimit.UsedMarginWithoutPL: {preLimit.UsedMarginWithoutPL}\n" +
                $"preLimit.Charges: {preLimit.Charges}\n" +
                $"postLimit.UsedMarginWithoutPL: {postLimit.UsedMarginWithoutPL}\n" +
                $"postLimit.Charges: {postLimit.Charges}\n" +
                $"orderMargin.OrderMargin: {orderMargin.OrderMargin}\n" +
                $"orderMargin.charges: {orderMargin.Charges}\n" +
                $"orderMargin.MarginUsedPrev: {orderMargin.MarginUsedPrev}";

                debugMessage.Warn();

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b) =>
                    Math.Abs(Normalize(a) - Normalize(b)) <= 0.01m;

                // marginusedprev check
                if (!AreEqual(orderMargin.MarginUsedPrev, preLimit.UsedMarginWithoutPL))
                {
                    errors.Add(
                        $"marginusedprev mismatch. Expected: {preLimit.UsedMarginWithoutPL}, Actual: {orderMargin.MarginUsedPrev}");
                }

                //internal movement check
                decimal remainingDelta =
                    Normalize(preLimit.RemainingMargin - postLimit.RemainingMargin);

                decimal usedDelta =
                    Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                if (!AreEqual(remainingDelta, usedDelta))
                {
                    errors.Add(
                        $"Limit delta mismatch. RemainingDelta: {remainingDelta}, UsedDelta: {usedDelta}");
                }

                //charge increase (soft validation)

                decimal chargeDelta = Normalize(postLimit.Charges - preLimit.Charges);

                decimal expectedChargeDelta;

                var previousOrderMargin = _cache.Get<OrderMarginDetails>(Constants.PreviousOrderMargin);

                // If previous order margin exists → MODIFY case
                if (previousOrderMargin != null)
                {
                    expectedChargeDelta = Normalize(
                        orderMargin.Charges - previousOrderMargin.Charges);
                }
                else
                    expectedChargeDelta = Normalize(orderMargin.Charges);

                if (!AreEqual(chargeDelta, expectedChargeDelta))
                {
                    errors.Add(
                        $"Charge mismatch. Expected: {expectedChargeDelta}, Actual: {chargeDelta}");
                }

                //UsedMargin internal consistency
                if (!AreEqual(postLimit.UsedMargin,
                    postLimit.UsedMarginWithoutCharges + postLimit.Charges))
                {
                    errors.Add("UsedMargin internal calculation mismatch");
                }

                //Transferable & Withdrawable
                if (!AreEqual(postLimit.TransferableAmount, postLimit.RemainingMargin))
                    errors.Add("TransferableAmount mismatch");

                if (!AreEqual(postLimit.WithdrawableAmount, postLimit.RemainingMargin))
                    errors.Add("WithdrawableAmount mismatch");

                //TotalCash stability
                if (!AreEqual(preLimit.TotalCash, postLimit.TotalCash))
                    errors.Add("TotalCash changed unexpectedly");

                //Percentage log
                decimal calculatedUsedPercent =
                    Normalize((postLimit.UsedMargin / postLimit.TotalCash) * 100);

                $"UsedMarginPercentage API : {postLimit.UsedMarginPercentage}".Warn();
                $"UsedMarginPercentage CALC: {calculatedUsedPercent}".Warn();

                _cache.Set("PostLimitMargin", postLimit);

                if (errors.Any())
                {
                    var message = string.Join(" | ", errors);
                    return message.FailWithLog(false);
                }

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error in ExtractPostLimitMarginHandler: {ex.Message}"
                    .FailWithLog();
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