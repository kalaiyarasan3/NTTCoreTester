using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System.Linq;

namespace NTTCoreTester.Activities
{
    /*
    If the position becomes zero, the square-off order must be properly completed (Filled). 
    Otherwise, the validation fails.
    */
    public class ValidateSquareOffOrderStatusHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => "ValidateSquareOffOrderStatus";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var postPositions = cache.Get<List<PositionBookModel>>(Constants.PrePositions);

                if (postPositions == null)
                    return "Post positions missing in cache"
                        .FailWithLog(true);

                string? symbol = cache.Get<string>(Constants.OrderSymbol);
                string? product = cache.Get<string>(Constants.OrderProduct);

                var position = postPositions
                    .FirstOrDefault(p => p.Symbol == symbol &&
                                         p.ProductType == product);

                if (position == null)
                    return "Position not found for square-off validation"
                        .FailWithLog(true);

                //Only validate order status if position is fully squared off
                if (position.NetQty != 0)
                {
                    return "Position not zero yet. Skipping order status validation.".FailWithLog();
                }

                var ordersToken = result.DataObject?["AllOrders"];

                if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                    return "AllOrders not found"
                        .FailWithLog(true);

                var orders = ordersToken.ToObject<List<OrderDetails>>();

                string? clientOrdId = cache.Get<string>(Constants.ClientOrdId);

                var relatedOrders = orders?
                    .Where(o => o.ClientOrderId == clientOrdId)
                    .ToList();

                if (relatedOrders == null || !relatedOrders.Any())
                    return $"Order {clientOrdId} not found"
                        .FailWithLog(true);

                var invalidOrders = relatedOrders.Where(o => o.Status != "1118").ToList();

                if (invalidOrders.Any())
                {
                    var states = string.Join(" | ",
                        invalidOrders.Select(o => o.Status));

                    return $"Position is zero but square-off order not in FILLED state. Current states: {states}"
                        .FailWithLog();
                }

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error in ValidateSquareOffOrderStatusHandler: {ex.Message}"
                    .FailWithLog(true);
            }
        }
    }
}