using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;
using System.Text;

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
                var debug = new StringBuilder();

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
                var order = _cache.Get<OrderDetails>(Constants.Order);

                decimal Normalize(decimal v)
                    => Math.Round(v, 2, MidpointRounding.AwayFromZero);

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

                AddDebug(debug,
                    $"Margin snapshot | " +
                    $"OrderMargin:{orderMargin?.OrderMargin} | Charges:{orderMargin?.Charges} | " +
                    $"PreUsed:{preUsed} | PostUsed:{postUsed} | " +
                    $"PreRemaining:{preRemaining} | PostRemaining:{postRemaining} | " +
                    $"UsedDelta:{usedDelta} | RemainingDelta:{remainingDelta} | " +
                    $"BalanceCheck:{deltaDiff} | Tolerance:{tolerance}");

                if (deltaDiff > tolerance)
                {
                    AddDebug(debug,
                        $"Margin accounting check | UsedDelta:{usedDelta} | RemainingDelta:{remainingDelta} | Sum:{usedDelta + remainingDelta}");
                }

                //---------------------------------------------------------
                // Preview Consistency
                //---------------------------------------------------------

                if (orderMargin != null)
                {
                    if (orderMargin.MarginUsedPrev > preUsed + 0.50m)
                    {
                        LogMarginComparison(
                            debug,
                            "Preview margin inconsistency",
                            orderMargin.MarginUsedPrev,
                            preUsed,
                            tolerance);
                    }
                }

                //---------------------------------------------------------
                // Order Margin Validation
                //---------------------------------------------------------

                if (order != null)
                {
                    ValidateOrderMargin(
                        order,
                        preLimit,
                        postLimit,
                        orderMargin,
                        tolerance,
                        usedDelta,
                        debug);
                }

                //---------------------------------------------------------
                // Position Exposure Validation
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
                        debug);
                }

                //---------------------------------------------------------
                // Insufficient Margin Check
                //---------------------------------------------------------

                if (orderMargin != null &&
                    preRemaining < orderMargin.OrderMargin &&
                    postUsed > preUsed)
                {
                    LogMarginComparison(
                        debug,
                        "Order executed despite insufficient margin",
                        orderMargin.OrderMargin,
                        preRemaining,
                        tolerance);
                }

                //---------------------------------------------------------
                // Internal Margin Calculation
                //---------------------------------------------------------

                decimal calculatedUsed =
                    Normalize(postLimit.UsedMarginWithoutCharges + postLimit.Charges);

                if (Math.Abs(postLimit.UsedMargin - calculatedUsed) > 0.02m)
                {
                    LogMarginComparison(
                        debug,
                        "UsedMargin calculation mismatch",
                        calculatedUsed,
                        postLimit.UsedMargin,
                        tolerance);
                }

                //---------------------------------------------------------
                // Cache Cleanup
                //---------------------------------------------------------

                _cache.Set(Constants.PostLimitMargin, postLimit);
                _cache.Remove(Constants.GetOrderMargin);
                _cache.Remove(Constants.ClientOrdIds);
                _cache.Remove(Constants.FilledQtyBySymbol);
                _cache.Remove(Constants.Order);

                return ActivityResult.Success(debug.ToString());
            }
            catch (Exception ex)
            {
                $"Error in ExtractPostLimitMargin: {ex.Message}".Warn();
                throw;
            }
        }

        //---------------------------------------------------------
        // Order Validation
        //---------------------------------------------------------

        private void ValidateOrderMargin(
            OrderDetails order,
            LimitMarginDetails preLimit,
            LimitMarginDetails postLimit,
            OrderMarginDetails orderMargin,
            decimal tolerance,
            decimal usedDelta,
            StringBuilder debug)
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

                LogMarginComparison(
                    debug,
                    "Rejected order margin behaviour",
                    pre,
                    post,
                    tolerance);

                return;
            }

            //---------------------------------------------------------
            // Active Orders
            //---------------------------------------------------------

            if (orderMargin != null &&
                order.OrderStatus is
                OrderEnumStatus.ORDER_PENDING or
                OrderEnumStatus.ORDER_TRADED)
            {
                decimal preview = Normalize(orderMargin.OrderMargin);

                LogMarginComparison(
                    debug,
                    "Preview vs Actual margin",
                    preview,
                    usedDelta,
                    tolerance);
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
            StringBuilder debug)
        {
            var scenario = DetectExposureChange(prePositions, postPositions);

            decimal change = postUsed - preUsed;

            AddDebug(debug, $"Exposure scenario:{scenario} | MarginDelta:{change}");

            switch (scenario)
            {
                case ExposureChangeType.ExposureIncrease:

                    if (change < 0)
                    {
                        AddDebug(debug,
                            "Exposure increased but margin decreased (pending margin release / rebalance)");
                    }
                    else if (change == 0)
                    {
                        AddDebug(debug,
                            "Exposure increased but margin unchanged (portfolio offset)");
                    }

                    break;

                case ExposureChangeType.ExposureDecrease:

                    if (change >= 0)
                    {
                        AddDebug(debug,
                            "Exposure decreased but margin unchanged (charges / hedge benefit)");
                    }

                    break;

                case ExposureChangeType.Flip:

                    if (Math.Abs(change) <= tolerance)
                    {
                        AddDebug(debug,
                            $"Position flip detected | MarginChange:{change} | Tolerance:{tolerance}");
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
                    increase = true;

                if (preQty != 0 && postQty == 0)
                    decrease = true;

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

        //---------------------------------------------------------
        // Debug Helpers
        //---------------------------------------------------------

        private void AddDebug(StringBuilder debug, string message)
        {
            debug.Append($" | {message}");
            message.Warn();
        }

        private void LogMarginComparison(
            StringBuilder debug,
            string title,
            decimal expected,
            decimal actual,
            decimal tolerance)
        {
            decimal diff = Math.Round(actual - expected, 2);

            AddDebug(debug,
                $"{title} | Expected:{expected} | Actual:{actual} | Diff:{diff} | Tolerance:{tolerance}");
        }
    }
}