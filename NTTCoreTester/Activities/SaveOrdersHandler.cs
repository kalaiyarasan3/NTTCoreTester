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
    public class SaveOrdersHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public SaveOrdersHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "SaveOrders";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;
            if (dataObject == null)
                return "SaveOrders: DataObject is null".FailWithLog();

            var ordersToken = dataObject["AllOrders"];
            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "SaveOrders: AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();
            var key = _cache.Get<string>(Constants.ClientOrdId);

            var order = orders?
                .Where(o => o.ClientOrderId == key
                         || o.NewClientOrderId == key
                         || o.OriginalClientOrderId == key)
                .OrderByDescending(o => o.OrderId)
                .FirstOrDefault();

            if (order == null)
                return $"SaveOrders: cl_ord_id={key} not found in OrderBook".FailWithLog();

            _cache.Set(Constants.OrderSymbol, order.TypeSymbol);
            _cache.Set(Constants.OrderProduct, order.Product);
            _cache.Set(Constants.OrderSide, order.TransactionType);
            _cache.Set(Constants.TotalQuantity, order.Quantity);
            _cache.Set(Constants.OrderBookAddedOn, order.AddedOn);


            $"SaveOrders: tsym={order.TypeSymbol} qty={order.Quantity} prd={order.Product} trantype={order.TransactionType}".Info();

            return ActivityResult.Success();
        }
    }
}
