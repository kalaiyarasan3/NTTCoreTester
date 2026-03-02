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
                    return "Post Limit margin not found".FailWithLog(true);

                var preLimit = _cache.Get<LimitMarginDetails>(Constants.PreLimitMargin);
                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog(true);

                if (orderMargin == null)
                    return "OrderMargin missing".FailWithLog(true);

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b) =>
                    Math.Abs(Normalize(a) - Normalize(b)) <= 0.01m;

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

                // ------------------------------------------------------------------
                // Margin Preview Consistency
                // ------------------------------------------------------------------

                if (!AreEqual(orderMargin.MarginUsedPrev, preLimit.UsedMarginWithoutPL))
                {
                    errors.Add(
                        $"MarginUsedPrev mismatch. Expected: {preLimit.UsedMarginWithoutPL}, Actual: {orderMargin.MarginUsedPrev}");
                }

                // ------------------------------------------------------------------
                // Delta Reconciliation
                // ------------------------------------------------------------------

                //excluding charges
                decimal usedWithoutPLDelta = Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                decimal remainingDelta = Normalize(postLimit.RemainingMargin - preLimit.RemainingMargin);

                if (!AreEqual(remainingDelta, -usedWithoutPLDelta))
                {
                    errors.Add(
                        $"Limit delta mismatch. " +
                        $"RemainingDelta: {remainingDelta}, " +
                        $"Expected: {-usedWithoutPLDelta}, " +
                        $"Difference: {Normalize(remainingDelta + usedWithoutPLDelta)}");
                }

                bool shouldBlockMargin = _cache.Get<bool>(Constants.ShouldBlockMargin);

                decimal actualBlocked = usedWithoutPLDelta;

                decimal chargeDelta = Normalize(postLimit.Charges - preLimit.Charges);

                if (shouldBlockMargin)
                {
                    decimal previewMargin = Normalize(orderMargin.OrderMargin);

                    if (!AreEqual(actualBlocked, previewMargin))
                    {
                        decimal difference = Normalize(previewMargin - actualBlocked);

                        errors.Add(
                            $"Preview vs Actual blocked margin mismatch. " +
                            $"Preview: {previewMargin}, Actual: {actualBlocked}, " +
                            $"Difference: {difference}");
                    }
                }
                else
                {
                    if (!AreEqual(actualBlocked, 0))
                    {
                        errors.Add(
                            "Margin blocked even though order was not accepted.");
                    }

                    if (!AreEqual(chargeDelta, 0))
                    {
                        errors.Add(
                            "Charges applied even though order was rejected.");
                    }
                }

                // ------------------------------------------------------------------
                // Internal UsedMargin Consistency
                // ------------------------------------------------------------------

                if (!AreEqual(postLimit.UsedMargin,
                    postLimit.UsedMarginWithoutCharges + postLimit.Charges))
                {
                    errors.Add("UsedMargin internal calculation mismatch");
                }

                // ------------------------------------------------------------------
                // Transferable / Withdrawable Consistency
                // ------------------------------------------------------------------

                if (!AreEqual(postLimit.TransferableAmount, postLimit.RemainingMargin))
                    errors.Add("TransferableAmount mismatch");

                if (!AreEqual(postLimit.WithdrawableAmount, postLimit.RemainingMargin))
                    errors.Add("WithdrawableAmount mismatch");

                if (!AreEqual(preLimit.TotalCash, postLimit.TotalCash))
                    errors.Add("TotalCash changed unexpectedly");

                // ------------------------------------------------------------------
                // Position-Based Validations
                // ------------------------------------------------------------------

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                var postPositions = _cache.Get<List<PositionBookModel>>(Constants.PostPositions);

                if (prePositions != null && postPositions != null)
                {
                    var symbol = _cache.Get<string>(Constants.OrderSymbol);
                    var product = _cache.Get<string>(Constants.OrderProduct);
                    var side = _cache.Get<string>(Constants.OrderSide);
                    int orderQty = _cache.Get<int>(Constants.TotalQuantity);

                    var prePosition = prePositions.FirstOrDefault(p =>
                        p.Symbol == symbol && p.ProductType == product);

                    var postPosition = postPositions.FirstOrDefault(p =>
                        p.Symbol == symbol && p.ProductType == product);

                    int preQty = prePosition?.NetQty ?? 0;
                    int postQty = postPosition?.NetQty ?? 0;

                    // ------------------------------------------------------------------
                    // Square-Off Detection (non-zero → zero)
                    // ------------------------------------------------------------------

                    if (preQty != 0 && postQty == 0)
                    {
                        decimal usedBefore = Normalize(preLimit.UsedMarginWithoutPL);
                        decimal usedAfter = Normalize(postLimit.UsedMarginWithoutPL);

                        if (Normalize(usedBefore - usedAfter) <= 0)
                        {
                            errors.Add(
                                $"Square-off margin release failed. " +
                                $"Before: {usedBefore}, After: {usedAfter}");
                        }
                    }

                    // ------------------------------------------------------------------
                    // Fresh Exposure Validation (Flip Case)
                    // Example: -2 + Buy 3 → +1 (1 fresh lot)
                    // ------------------------------------------------------------------

                    if (side != null)
                    {
                        int signedOrderQty = side.Equals("Buy", StringComparison.OrdinalIgnoreCase)
                            ? orderQty
                            : -orderQty;

                        int netAfter = preQty + signedOrderQty;

                        if (preQty != 0 && Math.Sign(preQty) != Math.Sign(netAfter))
                        {
                            int closingQty = Math.Min(Math.Abs(preQty), orderQty);
                            int freshQty = orderQty - closingQty;

                            if (freshQty > 0)
                            {
                                decimal marginPerLot =
                                    Normalize(orderMargin.OrderMargin / orderQty);

                                decimal requiredFreshMargin =
                                    Normalize(marginPerLot * freshQty);

                                if (preLimit.RemainingMargin < requiredFreshMargin &&
                                    postLimit.UsedMarginWithoutPL > preLimit.UsedMarginWithoutPL)
                                {
                                    errors.Add(
                                        $"RMS allowed fresh exposure without sufficient margin. " +
                                        $"FreshQty: {freshQty}, Required: {requiredFreshMargin}, " +
                                        $"Available: {preLimit.RemainingMargin}");
                                }
                            }
                        }
                    }
                }

                // ------------------------------------------------------------------
                // Full Insufficient Margin Validation
                // ------------------------------------------------------------------

                if (preLimit.RemainingMargin < orderMargin.OrderMargin &&
                    postLimit.UsedMarginWithoutPL > preLimit.UsedMarginWithoutPL)
                {
                    errors.Add(
                        $"RMS allowed execution despite insufficient margin. " +
                        $"Available: {preLimit.RemainingMargin}, " +
                        $"Required: {orderMargin.OrderMargin}");
                }

                _cache.Set(Constants.PostLimitMargin, postLimit);
                _cache.Set(Constants.ShouldBlockMargin, false);

                if (errors.Any())
                {
                    return string.Join(" | ", errors).FailWithLog(false);
                }

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                $"Error in ExtractPostLimitMarginHandler: {ex.Message} {ex.InnerException}".Warn();
                throw;
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