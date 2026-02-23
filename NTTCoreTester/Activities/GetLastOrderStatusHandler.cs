using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class GetLastOrderStatusHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public GetLastOrderStatusHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "GetLastOrderStatus";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "ResponseDataObject not found".FailWithLog();

            var ordersToken = dataObject["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var key = _cache.Get<string>(Constants.ClientOrdId);
             
            var relatedOrders = orders?
                .Where(x => x.ClientOrderId == key)
                .ToList();

            if (relatedOrders == null || !relatedOrders.Any())
                return $"Order {key} not found".FailWithLog();

            var pendingOrder = relatedOrders
                .FirstOrDefault(x => x.Status == "1111");

            if (pendingOrder == null)
            {
                var statuses = string.Join(",",
                    relatedOrders.Select(x => x.Status).Distinct());

                return $"1111 not found. Current statuses: {statuses}"
                    .FailWithLog();
            }

            _cache.Set(Constants.OrderNumber, pendingOrder.OrderNumber);
            _cache.Set(Constants.TotalQuantity, pendingOrder.Quantity);
            _cache.Set(Constants.OrderSymbol, pendingOrder.TypeSymbol);
            _cache.Set(Constants.OrderProduct, pendingOrder.Product);
            _cache.Set(Constants.OrderSide, pendingOrder.TransactionType);

            $"ordno: {pendingOrder.OrderNumber} qty: {pendingOrder.Quantity} symbol: {pendingOrder.TypeSymbol} product: {pendingOrder.Product} type: {pendingOrder.TransactionType}".Info();

            return ActivityResult.Success(pendingOrder.Remarks ?? "");
        }
    }

}

/*

Login 
Check Login
Get Security Info
Limits
Place Order
Check Order Status
Modify Order
place order with new client order id
cancel order
Square off order



 */