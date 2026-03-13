using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    /// <summary>
    /// checking the order status.
    /// set contions if not rejected for it should  block margin
    /// </summary>
    /// <param name="cache"></param>
    public class OrderStatusHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(OrderStatusHandler);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "ResponseDataObject not found".FailWithLog();

            var ordersToken = dataObject["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var key = cache.Get<string>(Constants.ClientOrdId);

            $"Client order id: {key}".Warn();

            var relatedOrders = orders?
            .Where(x =>
                x.NewClientOrderId == key ||
                x.OriginalClientOrderId == key ||
                x.ClientOrderId == key)
            .OrderByDescending(x => x.AddedOn)
            .ToList();

            if (relatedOrders == null || !relatedOrders.Any())
                return $"Order {key} not found".FailWithLog();

            var order = relatedOrders.FirstOrDefault();
            if (order == null)
                return $"Order {key} not found".FailWithLog();

            cache.Set(Constants.OrderNumber, order.OrderNumber);
            cache.Set(Constants.TotalQuantity, order.Quantity);
            cache.Set(Constants.OrderSymbol, order.TypeSymbol);
            cache.Set(Constants.OrderProduct, order.Product);
            cache.Set(Constants.OrderSide, order.TransactionType);
            cache.Set(Constants.Order, order);

            var map = cache.Get<Dictionary<string, string?>>(Constants.ClientOrdIds) ?? [];

            var clientOrderId = order.NewClientOrderId ?? order.ClientOrderId;

            map[$"{order.TypeSymbol}-{order.Product}"] = clientOrderId;

            cache.Set(Constants.ClientOrdIds, map);

            var log = $"status: {order.ExchangeStatus}, Product: {order.Product}, type: {order.TransactionType}, ordno:{order.OrderNumber}, qty:{order.Quantity}";
            log.Warn();
            switch (order.OrderStatus)
            {
                case OrderEnumStatus.ORDER_RECEIVED:
                case OrderEnumStatus.ORDER_PENDING:
                case OrderEnumStatus.ORDER_MODIFIED:
                case OrderEnumStatus.ORDER_TRADED:

                    return ActivityResult.Success(log);

                case OrderEnumStatus.ORDER_CANCELLED:
                case OrderEnumStatus.RMS_PENDING:
                case OrderEnumStatus.ORDER_REJECTED:
                case OrderEnumStatus.RMS_ORDER_REJECTED:
                case OrderEnumStatus.NSE_ADAPTOR_REJECTION:

                    cache.Set(Constants.ShouldBlockMargin, false);

                    return ActivityResult.Success(log);

                default:
                    cache.Set(Constants.ShouldBlockMargin, false);
                    return $"Unhandled order status: {order.Status} {log}"
                        .FailWithLog(false);
            }
        }
    }
}