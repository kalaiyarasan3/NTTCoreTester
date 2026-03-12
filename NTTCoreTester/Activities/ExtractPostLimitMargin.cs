using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractPostLimitMargin(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractPostLimitMargin);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
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

                if (preLimit == null)
                    return "PreLimitMargin missing".FailWithLog(true);

                var orderMargin = _cache.Get<OrderMarginDetails>(Constants.GetOrderMargin);
                bool hasOrderMargin = orderMargin != null;

                decimal Normalize(decimal value) =>
                    Math.Round(value, 2, MidpointRounding.AwayFromZero);

                bool AreEqual(decimal a, decimal b) =>
                    Math.Abs(Normalize(a) - Normalize(b)) <= 0.02m;

                // ----------------------------------------------------
                // Debug Information
                // ----------------------------------------------------

                var debug =
                    $"Margin snapshot " +
                    $"PreUsed:{preLimit.UsedMarginWithoutPL} | PostUsed:{postLimit.UsedMarginWithoutPL} | " +
                    $"PreRemaining:{preLimit.RemainingMargin} | PostRemaining:{postLimit.RemainingMargin}";

                if (hasOrderMargin)
                {
                    debug += $" | OrderMargin:{orderMargin.OrderMargin} | MarginUsedPrev:{orderMargin.MarginUsedPrev}";
                }

                // ----------------------------------------------------
                // Preview Consistency
                // ----------------------------------------------------

                if (hasOrderMargin && orderMargin.MarginUsedPrev > preLimit.UsedMarginWithoutPL + 0.10m)
                {
                    errors.Add(
                        $"MarginUsedPrev inconsistent. Prev:{orderMargin.MarginUsedPrev} Current:{preLimit.UsedMarginWithoutPL}");
                }

                // ----------------------------------------------------
                // Delta Reconciliation
                // ----------------------------------------------------

                decimal usedDelta =
                    Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                decimal remainingDelta =
                    Normalize(postLimit.RemainingMargin - preLimit.RemainingMargin);

                decimal deltaDiff = Math.Abs(remainingDelta + usedDelta);

                // 5% tolerance
                decimal tolerance = Math.Max(2m, preLimit.UsedMarginWithoutPL * 0.05m);

                debug += $" | Margin movement UsedDelta:{usedDelta} | RemainingDelta:{remainingDelta} | Difference:{deltaDiff} | Tolerance:{tolerance}";

                debug.Warn();

                if (deltaDiff > tolerance)
                {
                    errors.Add(
                        $"Limit delta mismatch. RemainingDelta:{remainingDelta} UsedDelta:{usedDelta}");
                }

                //----------------------------------------------------
                // Pending / Rejected Order Validation
                //----------------------------------------------------

                var order = _cache.Get<OrderDetails>(Constants.Order);
                if (order != null)
                {
                    var prePositionsTmp = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);

                    int preQty = prePositionsTmp?
                        .FirstOrDefault(p =>
                            p.Symbol == order.TypeSymbol &&
                            p.ProductType == order.Product)?
                        .NetQty ?? 0;

                    bool isRejectedOrFinal =
                        order.OrderStatus is
                            OrderEnumStatus.ORDER_CANCELLED or
                            OrderEnumStatus.ORDER_REJECTED or
                            OrderEnumStatus.RMS_PENDING or
                            OrderEnumStatus.RMS_ORDER_REJECTED or
                            OrderEnumStatus.NSE_ADAPTOR_REJECTION or
                            OrderEnumStatus.NOT_FOUND or
                            OrderEnumStatus.TRANSACTION_NOT_ALLOWED;

                    //----------------------------------------------------
                    // REJECTED / FINAL ORDER
                    //----------------------------------------------------

                    if (isRejectedOrFinal)
                    {
                        if (!AreEqual(postLimit.UsedMarginWithoutPL, preLimit.UsedMarginWithoutPL))
                        {
                            errors.Add(
                                $"Rejected order changed margin unexpectedly. " +
                                $"Status:{order.OrderStatus} " +
                                $"Pre:{preLimit.UsedMarginWithoutPL} Post:{postLimit.UsedMarginWithoutPL}");
                        }
                    }

                    //----------------------------------------------------
                    // PENDING / ACTIVE ORDER
                    //----------------------------------------------------

                    if (order.OrderStatus is
                        OrderEnumStatus.ORDER_PENDING or
                        OrderEnumStatus.ORDER_RECEIVED or
                        OrderEnumStatus.ORDER_MODIFIED or
                        OrderEnumStatus.ORDER_TRADED)
                    {
                        bool exposureIncrease = false;

                        if (order.TransactionType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
                            exposureIncrease = preQty >= 0;

                        if (order.TransactionType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
                            exposureIncrease = preQty <= 0;

                        if (exposureIncrease)
                        {
                            decimal actualBlocked =
                                Normalize(postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL);

                            if (actualBlocked <= 0)
                            {
                                errors.Add(
                                    $"Pending order should block margin but did not. " +
                                    $"Pre:{preLimit.UsedMarginWithoutPL} Post:{postLimit.UsedMarginWithoutPL}");
                            }

                            // Compare preview margin with actual blocked margin
                            if (hasOrderMargin)
                            {
                                decimal expected = Normalize(orderMargin.OrderMargin);

                                if (actualBlocked < expected - tolerance)
                                {
                                    errors.Add(
                                        $"Blocked margin less than preview. " +
                                        $"Expected:{expected} Actual:{actualBlocked}");
                                }
                            }
                        }
                        else
                        {
                            if (postLimit.UsedMarginWithoutPL > preLimit.UsedMarginWithoutPL + tolerance)
                            {
                                errors.Add(
                                    $"Closing pending order unexpectedly blocked margin. " +
                                    $"Pre:{preLimit.UsedMarginWithoutPL} Post:{postLimit.UsedMarginWithoutPL}");
                            }
                        }
                    }
                    _cache.Remove(Constants.Order);
                }

                // ----------------------------------------------------
                // Position Based Validation
                // ----------------------------------------------------

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                var postPositions = _cache.Get<List<PositionBookModel>>(Constants.PostPositions);

                if (prePositions != null && postPositions != null)
                {

                    var scenario = DetectExposureChange(prePositions, postPositions);

                    bool allClosedBefore = prePositions.Any(p => p.NetQty != 0);
                    bool allClosedAfter = postPositions.All(p => p.NetQty == 0);

                    if (allClosedBefore && allClosedAfter)
                    {
                        scenario = ExposureChangeType.SquareOff;
                    }

                    decimal change = postLimit.UsedMarginWithoutPL - preLimit.UsedMarginWithoutPL;

                    switch (scenario)
                    {
                        case ExposureChangeType.ExposureIncrease:

                            if (change <= 0)
                            {
                                errors.Add(
                                    $"Exposure increased but margin did not increase. " +
                                    $"Before:{preLimit.UsedMarginWithoutPL} After:{postLimit.UsedMarginWithoutPL}");
                            }

                            break;

                        case ExposureChangeType.ExposureDecrease:

                            if (change >= -tolerance)
                            {
                                errors.Add(
                                    $"Exposure decreased but margin did not reduce.");
                            }

                            break;

                        case ExposureChangeType.SquareOff:

                            if (postLimit.UsedMarginWithoutPL > tolerance)
                            {
                                errors.Add(
                                    $"Square-off margin not released. " +
                                    $"Pre:{preLimit.UsedMarginWithoutPL} Post:{postLimit.UsedMarginWithoutPL}");
                            }

                            break;

                        case ExposureChangeType.Flip:

                            decimal diff = Math.Abs(change);

                            if (diff <= tolerance)
                            {
                                errors.Add(
                                    $"Position flip detected but margin did not change significantly diff:{diff}.");
                            }

                            break;
                    }
                     
                }

                // ----------------------------------------------------
                // RMS Insufficient Margin Check
                // ----------------------------------------------------

                if (hasOrderMargin &&
                     preLimit.RemainingMargin < orderMargin.OrderMargin &&
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
                    $"UsedMargin internal calculation mismatch. | " +
                    $"UsedMargin: {postLimit.UsedMargin} | " +
                    $"UsedMarginWithoutCharges: {postLimit.UsedMarginWithoutCharges} | " +
                    $"Charges: {postLimit.Charges} | CalculatedUsedMargin: {calculatedUsed}");
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
                _cache.Remove(Constants.GetOrderMargin);

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

        private ExposureChangeType DetectExposureChange(List<PositionBookModel> prePositions,
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

                // new position
                if (pre == null && postQty != 0)
                {
                    increase = true;
                    continue;
                }

                // square-off
                if (preQty != 0 && postQty == 0)
                {
                    decrease = true;
                    continue;
                }

                // increase / decrease
                if (Math.Sign(preQty) == Math.Sign(postQty))
                {
                    if (Math.Abs(postQty) > Math.Abs(preQty))
                        increase = true;

                    if (Math.Abs(postQty) < Math.Abs(preQty))
                        decrease = true;
                }

                // flip
                if (Math.Sign(preQty) != Math.Sign(postQty))
                    flip = true;
            }

            if (flip) return ExposureChangeType.Flip;
            if (increase) return ExposureChangeType.ExposureIncrease;
            if (decrease) return ExposureChangeType.ExposureDecrease;

            return ExposureChangeType.NoChange;
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