using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;
using System.Diagnostics;

namespace NTTCoreTester.Activities
{
    public class ExtractPostLimitMargin(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractPostLimitMargin);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payload)
        {
            try
            {
                var errors = new List<string>();

                var postLimit = GetPrimaryLimitMargin(result)?
                    .FirstOrDefault(x => x.TemplateId == 1);

                if (postLimit == null)
                    return "Post limit margin not found".FailWithLog(true);

                var preLimit = _cache
                    .Get<List<LimitMarginDetails>>(Constants.PreLimitMargin)?
                    .FirstOrDefault(x => x.TemplateId == 1);

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog(true);

                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);
                bool hasOrderMargin = orderMargin != null;

                decimal Normalize(decimal value)
                    => Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b)
                    => Math.Abs(Normalize(a) - Normalize(b)) <= 0.02m;

                //---------------------------------------------------------
                // Margin Snapshot
                //---------------------------------------------------------

                decimal preUsed = Normalize(preLimit.UsedMarginWithoutPL);
                decimal postUsed = Normalize(postLimit.UsedMarginWithoutPL);

                decimal preRemaining = Normalize(preLimit.RemainingMargin);
                decimal postRemaining = Normalize(postLimit.RemainingMargin);

                decimal usedDelta = Normalize(postUsed - preUsed);
                decimal remainingDelta = Normalize(postRemaining - preRemaining);

                decimal deltaDiff = Math.Abs(usedDelta + remainingDelta);

                decimal tolerance = Math.Max(0.05m, preUsed * 0.05m);

                var debug =
                    $"Margin snapshot " +
                    $"PreUsed:{preUsed} | PostUsed:{postUsed} | " +
                    $"PreRemaining:{preRemaining} | PostRemaining:{postRemaining} | " +
                    $"UsedDelta:{usedDelta} | RemainingDelta:{remainingDelta} | " +
                    $"BalanceCheck:{deltaDiff} | Tolerance:{tolerance}";

                $"[Limits] {debug}".Warn();

                if (deltaDiff > tolerance)
                {
                    errors.Add(
                        $"Margin accounting mismatch. UsedDelta:{usedDelta} RemainingDelta:{remainingDelta}");
                }

                //---------------------------------------------------------
                // Preview Consistency
                //---------------------------------------------------------

                if (hasOrderMargin)
                {
                    if (orderMargin.MarginUsedPrev > preUsed + 0.50m)
                    {
                        errors.Add(
                            $"Preview margin inconsistent. PreviewPrev:{orderMargin.MarginUsedPrev} Current:{preUsed}");
                    }
                }

                //---------------------------------------------------------
                // Order Based Validation
                //---------------------------------------------------------

                var order = _cache.Get<OrderDetails>(Constants.Order);

                if (order != null)
                {
                    ValidateOrderMargin(order, preLimit, postLimit, orderMargin,
                        tolerance, usedDelta, errors, ref debug);
                }

                //---------------------------------------------------------
                // Position Based Validation
                //---------------------------------------------------------

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                var postPositions = _cache.Get<List<PositionBookModel>>(Constants.PostPositions);

                if (prePositions != null && postPositions != null)
                {
                    ValidatePositionExposure(
                        prePositions,
                        postPositions,
                        preUsed,
                        postUsed,
                        tolerance,
                        errors,
                        ref debug);
                }

                //---------------------------------------------------------
                // Insufficient Margin Check
                //---------------------------------------------------------

                if (hasOrderMargin &&
                    preRemaining < orderMargin.OrderMargin &&
                    postUsed > preUsed)
                {
                    errors.Add(
                        $"Order executed despite insufficient margin. " +
                        $"Available:{preRemaining} Required:{orderMargin.OrderMargin}");
                }

                //---------------------------------------------------------
                // Internal Consistency
                //---------------------------------------------------------

                decimal calculatedUsed =
                    Normalize(postLimit.UsedMarginWithoutCharges + postLimit.Charges);

                if (!AreEqual(postLimit.UsedMargin, calculatedUsed))
                {
                    errors.Add(
                        $"UsedMargin calculation mismatch. " +
                        $"UsedMargin:{postLimit.UsedMargin} " +
                        $"Calculated:{calculatedUsed}");
                }

                //---------------------------------------------------------
                // Cache Cleanup
                //---------------------------------------------------------

                _cache.Set(Constants.PostLimitMargin, postLimit);
                _cache.Remove(Constants.GetOrderMargin);
                _cache.Remove(Constants.ClientOrdIds);
                _cache.Remove(Constants.FilledQtyBySymbol);
                _cache.Remove(Constants.Order);

                if (errors.Any())
                {
                    errors.Add($"Debug Info: {debug}");
                    return string.Join(" | ", errors).FailWithLog(false);
                }
                return ActivityResult.Success(debug);
            }
            catch (Exception ex)
            {
                $"Error in ExtractPostLimitMargin: {ex.Message}".Warn();
                throw;
            }
        }

        //---------------------------------------------------------
        // Order Margin Validation
        //---------------------------------------------------------

        private void ValidateOrderMargin(
            OrderDetails order,
            LimitMarginDetails preLimit,
            LimitMarginDetails postLimit,
            OrderMarginDetails orderMargin,
            decimal tolerance,
            decimal usedDelta,
            List<string> errors,
             ref string debug)
        {
            decimal Normalize(decimal v)
                => Math.Round(v, 2, MidpointRounding.AwayFromZero);

            bool rejected =
                order.OrderStatus is
                    OrderEnumStatus.ORDER_CANCELLED or
                    OrderEnumStatus.ORDER_REJECTED or
                    OrderEnumStatus.RMS_ORDER_REJECTED or
                    OrderEnumStatus.NSE_ADAPTOR_REJECTION or
                    OrderEnumStatus.TRANSACTION_NOT_ALLOWED;

            //---------------------------------------------------------
            // Rejected Orders
            //---------------------------------------------------------

            if (rejected)
            {
                decimal pre = Normalize(preLimit.UsedMarginWithoutCharges);
                decimal post = Normalize(postLimit.UsedMarginWithoutCharges);

                decimal diff = post - pre;

                if (Math.Abs(diff) > tolerance)
                {
                    errors.Add(
                        $"Rejected order changed margin. " +
                        $"Status:{order.OrderStatus} Pre:{pre} Post:{post} Diff:{diff}");
                }

                return;
            }

            //---------------------------------------------------------
            // Active Orders
            //---------------------------------------------------------

            if (orderMargin != null && order.OrderStatus is
                OrderEnumStatus.ORDER_PENDING or
                OrderEnumStatus.ORDER_TRADED)
            {
                decimal preview = Normalize(orderMargin.OrderMargin);

                if (usedDelta > preview + tolerance)
                {
                    errors.Add(
                        $"Blocked margin exceeds preview. Preview:{preview} Actual:{usedDelta}");
                }

                if (usedDelta < preview - tolerance)
                {
                    AddDebug(ref debug, $"Actual margin smaller than preview (portfolio rebalance likely). Preview:{preview} Actual:{usedDelta}");
                }
            }
        }

        //---------------------------------------------------------
        // Position Exposure Validation
        //---------------------------------------------------------

        private void ValidatePositionExposure(
            List<PositionBookModel> prePositions,
            List<PositionBookModel> postPositions,
            decimal preUsed,
            decimal postUsed,
            decimal tolerance,
            List<string> errors,
            ref string debug)
        {
            var scenario = DetectExposureChange(prePositions, postPositions);

            decimal change = postUsed - preUsed;
             
            AddDebug(ref debug, $"Exposure scenario: {scenario} | MarginDelta:{change}");
            switch (scenario)
            {
                case ExposureChangeType.ExposureIncrease:

                    if (change <= 0)
                    {
                        errors.Add(
                            $"Exposure increased but margin did not increase.");
                    }
                    break;

                case ExposureChangeType.ExposureDecrease:

                    if (change >= 0)
                    {
                        errors.Add(
                            $"Exposure decreased but margin did not reduce.");
                    }
                    break;

                case ExposureChangeType.Flip:

                    if (Math.Abs(change) <= tolerance)
                    {
                        AddDebug(ref debug, "Flip detected with minimal margin change (close + reopen scenario)");
                    }
                    break;
            }
        }

        //---------------------------------------------------------
        // Exposure Detection
        //---------------------------------------------------------

        private ExposureChangeType DetectExposureChange(
            List<PositionBookModel> prePositions,
            List<PositionBookModel> postPositions)
        {
            bool increase = false;
            bool decrease = false;
            bool flip = false;

            foreach (var post in postPositions)
            {
                var pre = prePositions.FirstOrDefault(p =>
                    p.Symbol == post.Symbol &&
                    p.ProductType == post.ProductType);

                int preQty = pre?.NetQty ?? 0;
                int postQty = post.NetQty;

                if (preQty == postQty)
                    continue;

                $"{post.Symbol}-{post.ProductType} position change : Pre:{preQty} Post:{postQty}".Warn();

                if (pre == null && postQty != 0)
                {
                    increase = true;
                    continue;
                }

                if (preQty != 0 && postQty == 0)
                {
                    decrease = true;
                    continue;
                }

                if (Math.Sign(preQty) == Math.Sign(postQty))
                {
                    if (Math.Abs(postQty) > Math.Abs(preQty))
                        increase = true;

                    if (Math.Abs(postQty) < Math.Abs(preQty))
                        decrease = true;
                }

                if (Math.Sign(preQty) != Math.Sign(postQty))
                    flip = true;
            }

            if (flip) return ExposureChangeType.Flip;
            if (increase) return ExposureChangeType.ExposureIncrease;
            if (decrease) return ExposureChangeType.ExposureDecrease;

            return ExposureChangeType.NoChange;
        }

        //---------------------------------------------------------
        // Limit Extractor
        //---------------------------------------------------------

        private List<LimitMarginDetails>? GetPrimaryLimitMargin(ApiExecutionResult result)
        {
            var margins = result.DataObject?[Constants.AllMargins];

            if (margins == null || margins.Type != JTokenType.Array)
                return null;

            return margins.ToObject<List<LimitMarginDetails>>();
        }

        void AddDebug(ref string debug, string message)
        {
            debug += $" | {message}";
            message.Warn();
        }
    }
}