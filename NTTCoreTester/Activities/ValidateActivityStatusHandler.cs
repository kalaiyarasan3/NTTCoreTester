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

    /// <summary>
    /// Validates that the ActivityOrderBook reflects the correct Filled status (1118)
    /// after a trade has been confirmed via TradeBook.
    /// 
    /// this hadler detects the bug where the activity section continues to pending even after the order is filled.
    /// 
    /// </summary>
    public class ValidateActivityStatusHandler:IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        private const int MaxRetries = 5;
        private const int RetryDelayMs = 2000;

        public string Name => "ValidateActivityStatus";

        public ValidateActivityStatusHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;
            if (dataObject == null)
                return "ResponseDataObject not found.".FailWithLog();

            var ordersToken = dataObject["AllOrders"];
            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found or invalid.".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();
            if (orders == null || !orders.Any())
                return "No orders returned.".FailWithLog();

            var key = _cache.Get<string>(Constants.ClientOrdId);
            if (string.IsNullOrEmpty(key))
                return "ClientOrdId not found in cache.".FailWithLog();

            var relatedOrders = orders
                .Where(x => x.ClientOrderId == key || x.NewClientOrderId == key)
                .ToList();

            if (!relatedOrders.Any())
                return $"Order [{key}] not found in ActivityOrderBook.".FailWithLog();

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                var filledOrder = relatedOrders.FirstOrDefault(x => x.Status == "1118");

                if (filledOrder != null)
                {
                    $"[Attempt {attempt}] Activity status correctly shows Filled (1118) for order [{key}].".Info();
                    return ActivityResult.Success(filledOrder.Remarks ?? string.Empty);
                }

                // Check what status is currently showing
                var currentStatuses = string.Join(", ", relatedOrders
                    .Select(x => $"{x.Status} ({x.Remarks ?? "No Remarks"})")
                    .Distinct());

                // ── Still Pending —  ──
                if (relatedOrders.Any(x => x.Status == "1111"))
                {
                    $"[Attempt {attempt}/{MaxRetries}] Activity still shows Pending (1111) after trade fill. Expected Filled (1118). Retrying in {RetryDelayMs}ms...".Warn();
                }
                else
                {
                    $"[Attempt {attempt}/{MaxRetries}] Unexpected status in Activity: {currentStatuses}. Retrying...".Warn();
                }

                if (attempt < MaxRetries)
                    Thread.Sleep(RetryDelayMs);
            }

            // ── After all retries, still not Filled — bug confirmed ──
            var finalStatuses = string.Join(", ", relatedOrders
                .Select(x => $"{x.Status} ({x.Remarks ?? "No Remarks"})")
                .Distinct());

            return $"[BUG] Activity section did not update to Filled (1118) after trade confirmation for order [{key}]. Still showing: {finalStatuses}"
                   .FailWithLog();
        }
    }
}
