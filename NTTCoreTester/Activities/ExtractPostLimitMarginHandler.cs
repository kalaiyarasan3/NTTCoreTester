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
                    return ActivityResult.HardFail("Post Limit margin not found");

                var preLimit = _cache.Get<LimitMarginDetails>(Constants.PreLimitMargin);
                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);

                if (preLimit == null)
                    return ActivityResult.HardFail("PreLimitMargin missing");

                if (orderMargin == null)
                    return ActivityResult.HardFail("OrderMargin missing");

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                //Expected deduction = ordermargin + charges

                decimal expectedTotalDeduction =
                    Normalize(orderMargin.OrderMargin + orderMargin.Charges);

                //RemainingMargin check

                decimal actualRemainingReduction =
                    Normalize(preLimit.RemainingMargin - postLimit.RemainingMargin);

                if (actualRemainingReduction != expectedTotalDeduction)
                {
                    return ActivityResult.SoftFail(
                        $"RemainingMargin mismatch. Expected: {expectedTotalDeduction}, Actual: {actualRemainingReduction}");
                }

                //UsedMargin check

                decimal actualUsedIncrease =
                    Normalize(postLimit.UsedMargin - preLimit.UsedMargin);

                if (actualUsedIncrease != expectedTotalDeduction)
                {
                    return ActivityResult.SoftFail(
                        $"UsedMargin mismatch. Expected: {expectedTotalDeduction}, Actual: {actualUsedIncrease}");
                }

                //UsedMarginWithoutCharges check

                decimal expectedWithoutCharges =
                    Normalize(orderMargin.OrderMargin);

                decimal actualWithoutChargesIncrease =
                    Normalize(postLimit.UsedMarginWithoutCharges -
                              preLimit.UsedMarginWithoutCharges);

                if (actualWithoutChargesIncrease != expectedWithoutCharges)
                {
                    return ActivityResult.SoftFail(
                        $"UsedMarginWithoutCharges mismatch. Expected: {expectedWithoutCharges}, Actual: {actualWithoutChargesIncrease}");
                }

                //Charges cumulative check

                decimal expectedCharges =
                    Normalize(preLimit.Charges + orderMargin.Charges);

                if (Normalize(postLimit.Charges) != expectedCharges)
                {
                    return ActivityResult.SoftFail(
                        $"Charges mismatch. Expected: {expectedCharges}, Actual: {postLimit.Charges}");
                }

                // UsedMargin internal consistency
                if (Normalize(postLimit.UsedMargin) !=
                    Normalize(postLimit.UsedMarginWithoutCharges + postLimit.Charges))
                {
                    return ActivityResult.SoftFail("UsedMargin internal calculation mismatch");
                }


                //Transferable & Withdrawable check

                if (Normalize(postLimit.TransferableAmount) !=
                    Normalize(postLimit.RemainingMargin))
                {
                    return ActivityResult.SoftFail("TransferableAmount mismatch");
                }

                if (Normalize(postLimit.WithdrawableAmount) !=
                    Normalize(postLimit.RemainingMargin))
                {
                    return ActivityResult.SoftFail("WithdrawableAmount mismatch");
                }

                //Percentage check

                decimal calculatedUsedPercent =
                    Normalize((postLimit.UsedMargin / postLimit.TotalCash) * 100);

                Console.WriteLine($"[INFO] UsedMarginPercentage API: {postLimit.UsedMarginPercentage}");
                Console.WriteLine($"[INFO] UsedMarginPercentage CALC: {calculatedUsedPercent}");


                if (Normalize(preLimit.TotalCash) != Normalize(postLimit.TotalCash))
                {
                    return ActivityResult.SoftFail("TotalCash changed unexpectedly");
                }

                //if mismatch report

                _cache.Set("PostLimitMargin", postLimit);

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail(
                    $"Error in ExtractPostLimitMarginHandler: {ex.Message}");
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
