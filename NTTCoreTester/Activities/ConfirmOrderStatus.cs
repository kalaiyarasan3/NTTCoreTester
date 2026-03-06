using Newtonsoft.Json.Linq;
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
    public class ConfirmOrderStatus(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ConfirmOrderStatus);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
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
                .Where(x => x.NewClientOrderId == key)
                .ToList();

            if (relatedOrders == null || !relatedOrders.Any())
                return $"Order {key} not found".FailWithLog();

            var filledOrder = relatedOrders
                .FirstOrDefault(x => x.Status == "1118");

            if (filledOrder == null)
            {
                if(relatedOrders.Any(x => x.Status == "1111" || x.Status =="0000"))
                {
                    $"Order is pending set block margin true".Warn();
                    cache.Set(Constants.ShouldBlockMargin, true);
                }
                var statuses = string.Join(" | ",
                    relatedOrders
                        .Select(x => $"[{x.Status}] {x.Remarks ?? "No Remarks"}")
                        .Distinct());

                return $"Current states: {statuses}"
                    .FailWithLog(false);
            }
            cache.Set(Constants.ShouldBlockMargin, true);

            return ActivityResult.Success(filledOrder.Remarks ?? "");
        }
    }
}
