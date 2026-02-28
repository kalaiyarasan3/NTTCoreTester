using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ValidateRejectedOrderImmutabilityHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ValidateRejectedOrderImmutabilityHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ValidateRejectedOrderImmutability";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var ordersToken = result.DataObject?["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var clientId = _cache.Get<string>(Constants.ClientOrdId);

            var related = orders?
                .Where(o => o.NewClientOrderId == clientId)
                .ToList();

            if (related == null || !related.Any())
                return "Order not found".FailWithLog();

            bool rejected = related.Any(o => o.Status == "0001");
            bool filled = related.Any(o => o.Status == "1118");

            var order = orders?.FirstOrDefault(x => x.NewClientOrderId == clientId);

            _cache.Set(Constants.OrderNumber, order.OrderNumber);
            _cache.Set(Constants.TotalQuantity, order.Quantity);
            _cache.Set(Constants.OrderSymbol, order.TypeSymbol);
            _cache.Set(Constants.OrderProduct, order.Product);
            _cache.Set(Constants.OrderSide, order.TransactionType);

            if (rejected && filled)
            {
                return "Invalid state transition: Rejected order became Filled."
                    .FailWithLog(true);
            }

            return ActivityResult.Success();
        }
    }
}
