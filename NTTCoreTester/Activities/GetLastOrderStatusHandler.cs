using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
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
                return ActivityResult.HardFail("ResponseDataObject not found");

            var ordersToken = dataObject["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return ActivityResult.HardFail("AllOrders not found");

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var key = _cache.Get(Constants.ClientOrdId);

            var order = orders?.FirstOrDefault(x => x.ClientOrderId == key);

            if (order == null)
                return ActivityResult.HardFail("Order not found");

            return ActivityResult.Success();
        }
    }

}