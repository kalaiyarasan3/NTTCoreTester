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

            var order = orders?.FirstOrDefault(x => x.ClientOrderId == key);

            if (order == null)
                return "Order not found".FailWithLog();

            if (order.Status != "1111")
            {
                return $"status: {order.Status}, Remarks: {order.Remarks}".FailWithLog();
            }

            _cache.Set(Constants.OrderNumber, order.OrderNumber);

            return ActivityResult.Success();
        }
    }

}