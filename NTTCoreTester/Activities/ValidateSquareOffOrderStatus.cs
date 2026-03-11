using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;
using System.Linq;

namespace NTTCoreTester.Activities
{
    /*
    If the position becomes zero, the square-off order must be properly completed (Filled). 
    Otherwise, the validation fails.
    */
    public class ValidateSquareOffOrderStatus(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ValidateSquareOffOrderStatus);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                List<string> errors = [];
                var postPositions = cache.Get<List<PositionBookModel>>(Constants.PostPositions);

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

                var clientOrdIds = cache.Get<List<string>>(Constants.ClientOrdIds);

                foreach (var orderId in clientOrdIds)
                {
                    var relatedOrders = orders
                        .Where(o => o.NewClientOrderId == orderId)
                        .ToList();

                    bool filled = relatedOrders.Any(o =>
                        o.OrderStatus == OrderEnumStatus.ORDER_TRADED);

                    if (!filled)
                    {
                        errors.Add($"Square-off order {orderId} not filled.");
                    }
                }
                string? clientOrdId = cache.Get<string>(Constants.ClientOrdId);

                if (errors.Any())
                {
                    var log = $"Square-off validation failed for ClientOrdId: {clientOrdId}. Errors: {string.Join(" | ", errors)}";
                    return log.FailWithLog();
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