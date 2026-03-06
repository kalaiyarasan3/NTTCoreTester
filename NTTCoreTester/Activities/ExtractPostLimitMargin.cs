using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractPostLimitMargin(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractPostLimitMargin);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var errors = new List<string>();

                var postLimits = GetPrimaryLimitMargin(result);
                var postLimit = postLimits?.FirstOrDefault(x => x.TemplateId == 1);

                if (postLimit == null)
                    return "Post limit margin not found".FailWithLog(true);

                var preLimit = _cache
                    .Get<List<LimitMarginDetails>>(Constants.PreLimitMargin)?
                    .FirstOrDefault(x => x.TemplateId == 1);

                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog(true);

                if (orderMargin == null)
                    return "OrderMargin missing".FailWithLog(true);

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b) =>
                    Math.Abs(Normalize(a) - Normalize(b)) <= 0.02m;

                // ----------------------------------------------------
                // Debug Information
                // ----------------------------------------------------

                var debug =
                    $"preLimit.RemainingMargin: {preLimit.RemainingMargin}\n" +
                    $"postLimit.RemainingMargin: {postLimit.RemainingMargin}\n" +
                    $"preLimit.UsedMarginWithoutPL: {preLimit.UsedMarginWithoutPL}\n" +
                    $"postLimit.UsedMarginWithoutPL: {postLimit.UsedMarginWithoutPL}\n" +
                    $"orderMargin.OrderMargin: {orderMargin.OrderMargin}\n" +
                    $"orderMargin.MarginUsedPrev: {orderMargin.MarginUsedPrev}";

                debug.Warn();

                // ----------------------------------------------------
                // Preview Consistency
                // ----------------------------------------------------

                if (!AreEqual(orderMargin.MarginUsedPrev, preLimit.UsedMarginWithoutPL))
                {
                    errors.Add(
                        $"MarginUsedPrev mismatch. Expected: {preLimit.UsedMarginWithoutPL}, " +
                        $"Actual: {orderMargin.MarginUsedPrev}");
                }

                // ----------------------------------------------------
                // Delta Reconciliation
                // ----------------------------------------------------

                decimal usedDelta =
                    Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                decimal remainingDelta =
                    Normalize(postLimit.RemainingMargin - preLimit.RemainingMargin);

                if (!AreEqual(remainingDelta, -usedDelta))
                {
                    errors.Add(
                        $"Limit delta mismatch. RemainingDelta: {remainingDelta}, Expected: {-usedDelta}");
                }

                // ----------------------------------------------------
                // Position Based Validation
                // ----------------------------------------------------

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                var postPositions = _cache.Get<List<PositionBookModel>>(Constants.PostPositions);

                if (prePositions != null && postPositions != null)
                {
                    var symbol = _cache.Get<string>(Constants.OrderSymbol);
                    var product = _cache.Get<string>(Constants.OrderProduct);

                    var prePos = prePositions.FirstOrDefault(p =>
                        p.Symbol == symbol && p.ProductType == product);

                    var postPos = postPositions.FirstOrDefault(p =>
                        p.Symbol == symbol && p.ProductType == product);

                    int preQty = prePos?.NetQty ?? 0;
                    int postQty = postPos?.NetQty ?? 0;

                    bool isFreshExposure = preQty == 0 && postQty != 0;

                    bool isFullSquareOff = preQty != 0 && postQty == 0;

                    bool isPartialClose =
                        preQty != 0 &&
                        postQty != 0 &&
                        Math.Sign(preQty) == Math.Sign(postQty) &&
                        Math.Abs(postQty) < Math.Abs(preQty);

                    bool isIncrease =
                        preQty != 0 &&
                        postQty != 0 &&
                        Math.Sign(preQty) == Math.Sign(postQty) &&
                        Math.Abs(postQty) > Math.Abs(preQty);

                    bool isPositionFlip =
                        preQty != 0 &&
                        postQty != 0 &&
                        Math.Sign(preQty) != Math.Sign(postQty);

                    // ---------------------------------------------

                    if (isFreshExposure || isIncrease)
                    {
                        if (postLimit.UsedMarginWithoutPL <= preLimit.UsedMarginWithoutPL)
                        {
                            errors.Add(
                                $"Fresh exposure detected but margin did not increase. " +
                                $"Before: {preLimit.UsedMarginWithoutPL}, After: {postLimit.UsedMarginWithoutPL}");
                        }
                    }

                    if (isPartialClose)
                    {
                        if (postLimit.UsedMarginWithoutPL >= preLimit.UsedMarginWithoutPL)
                        {
                            errors.Add(
                                $"Partial close detected but margin did not reduce. " +
                                $"Before: {preLimit.UsedMarginWithoutPL}, After: {postLimit.UsedMarginWithoutPL}");
                        }
                    }

                    if (isFullSquareOff)
                    {
                        if (postLimit.UsedMarginWithoutPL >= preLimit.UsedMarginWithoutPL)
                        {
                            errors.Add(
                                $"Square-off margin release failed. " +
                                $"Before: {preLimit.UsedMarginWithoutPL}, After: {postLimit.UsedMarginWithoutPL}");
                        }
                    }

                    if (isPositionFlip)
                    {
                        if (postLimit.UsedMarginWithoutPL <= 0)
                        {
                            errors.Add(
                                $"Position flip detected but margin not blocked for new exposure.");
                        }
                    }
                }

                // ----------------------------------------------------
                // RMS Insufficient Margin Check
                // ----------------------------------------------------

                if (preLimit.RemainingMargin < orderMargin.OrderMargin &&
                    postLimit.UsedMarginWithoutPL > preLimit.UsedMarginWithoutPL)
                {
                    errors.Add(
                        $"RMS allowed execution despite insufficient margin. " +
                        $"Available: {preLimit.RemainingMargin}, Required: {orderMargin.OrderMargin}");
                }

                // ----------------------------------------------------
                // Internal Consistency
                // ----------------------------------------------------

                decimal calculatedUsed =
                    Normalize(postLimit.UsedMarginWithoutCharges + postLimit.Charges);

                if (!AreEqual(postLimit.UsedMargin, calculatedUsed))
                {
                    errors.Add(
                        $"UsedMargin internal calculation mismatch. " +
                        $"UsedMargin: {postLimit.UsedMargin}, " +
                        $"UsedMarginWithoutCharges: {postLimit.UsedMarginWithoutCharges}, " +
                        $"Charges: {postLimit.Charges}, " +
                        $"CalculatedUsedMargin: {calculatedUsed}");
                }
                /*
                if (!AreEqual(postLimit.TransferableAmount, postLimit.RemainingMargin))
                {
                    errors.Add(
                        $"TransferableAmount mismatch. " +
                        $"TransferableAmount: {postLimit.TransferableAmount}, " +
                        $"RemainingMargin: {postLimit.RemainingMargin}");
                }

                if (!AreEqual(postLimit.WithdrawableAmount, postLimit.RemainingMargin))
                {
                    errors.Add(
                        $"WithdrawableAmount mismatch. " +
                        $"WithdrawableAmount: {postLimit.WithdrawableAmount}, " +
                        $"RemainingMargin: {postLimit.RemainingMargin}");
                }

                if (!AreEqual(preLimit.TotalCash, postLimit.TotalCash))
                {
                    errors.Add(
                        $"TotalCash changed unexpectedly. " +
                        $"PreTotalCash: {preLimit.TotalCash}, " +
                        $"PostTotalCash: {postLimit.TotalCash}");
                }
                */
                // ----------------------------------------------------

                _cache.Set(Constants.PostLimitMargin, postLimit);

                if (errors.Any())
                    return string.Join(" | ", errors).FailWithLog(false);

                return ActivityResult.Success(debug);
            }
            catch (Exception ex)
            {
                $"Error in ExtractPostLimitMargin: {ex.Message}".Warn();
                throw;
            }
        }

        private List<LimitMarginDetails>? GetPrimaryLimitMargin(ApiExecutionResult result)
        {
            var margins = result.DataObject?[Constants.AllMargins];

            if (margins == null || margins.Type != JTokenType.Array)
                return null;

            return margins.ToObject<List<LimitMarginDetails>>();
        }
    }
}