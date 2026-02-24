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
                var postLimit = GetPrimaryLimitMargin(result);

                if (postLimit == null)
                    return "Post Limit margin not found".FailWithLog();

                var preLimit = _cache.Get<LimitMarginDetails>(Constants.PreLimitMargin);
                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog();

                if (orderMargin == null)
                    return "OrderMargin missing".FailWithLog();

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b) =>
                    Math.Abs(Normalize(a) - Normalize(b)) <= 0.01m;
                 
                //  Validate marginusedprev matches PreLimit 

                if (!AreEqual(orderMargin.MarginUsedPrev, preLimit.UsedMarginWithoutPL))
                {
                    return $"marginusedprev mismatch. Expected: {preLimit.UsedMarginWithoutPL}, Actual: {orderMargin.MarginUsedPrev}"
                        .FailWithLog(false);
                }
                 
                // Validate internal margin movement 

                decimal remainingDelta =
                    Normalize(preLimit.RemainingMargin - postLimit.RemainingMargin);

                decimal usedDelta =
                    Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                if (!AreEqual(remainingDelta, usedDelta))
                {
                    return $"Limit delta mismatch. RemainingDelta: {remainingDelta}, UsedDelta: {usedDelta}"
                        .FailWithLog(false);
                }
                 
                // Validate charge increase only 

                decimal chargeDelta =
                    Normalize(postLimit.Charges - preLimit.Charges);

                if (!AreEqual(chargeDelta, orderMargin.Charges))
                {
                    return $"Charge mismatch. Expected: {orderMargin.Charges}, Actual: {chargeDelta}"
                        .FailWithLog(false);
                }
                 
                // UsedMargin internal consistency 

                if (!AreEqual(postLimit.UsedMargin,
                    postLimit.UsedMarginWithoutCharges + postLimit.Charges))
                {
                    return "UsedMargin internal calculation mismatch"
                        .FailWithLog(false);
                }
                 
                // Transferable & Withdrawable must equal Remaining 

                if (!AreEqual(postLimit.TransferableAmount, postLimit.RemainingMargin))
                    return "TransferableAmount mismatch".FailWithLog(false);

                if (!AreEqual(postLimit.WithdrawableAmount, postLimit.RemainingMargin))
                    return "WithdrawableAmount mismatch".FailWithLog(false);
                 
                // TotalCash must remain unchanged 

                if (!AreEqual(preLimit.TotalCash, postLimit.TotalCash))
                    return "TotalCash changed unexpectedly".FailWithLog(false);
                 
                // Percentage validation (Reference Only) 

                decimal calculatedUsedPercent =
                    Normalize((postLimit.UsedMargin / postLimit.TotalCash) * 100);

                Console.WriteLine($"[INFO] UsedMarginPercentage API : {postLimit.UsedMarginPercentage}");
                Console.WriteLine($"[INFO] UsedMarginPercentage CALC: {calculatedUsedPercent}");
                 
                _cache.Set("PostLimitMargin", postLimit);

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