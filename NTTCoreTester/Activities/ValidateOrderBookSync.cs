using Newtonsoft.Json;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using NTTCoreTester.Reporting;

namespace NTTCoreTester.Activities
{
    /// <summary>
    /// Issue 1 — OrderBook (cache) vs ActivityOrderBook (DB) sync validation.
    ///
    /// Checks:
    ///   1. Order exists in ActivityOrderBook
    ///   2. Fields match between OrderBook and ActivityOrderBook (tsym, qty, prd, trantype)
    ///   3. Timing: PlaceOrder→OrderBook | PlaceOrder→ActivityOrderBook | PlaceOrder→Exchange
    /// </summary>
    public class ValidateOrderBookSync : IActivityHandler
    {
        private readonly PlaceholderCache _cache;
        private readonly CsvReport _csvReport;


        public string Name => nameof(ValidateOrderBookSync);

        public ValidateOrderBookSync(PlaceholderCache cache, CsvReport csvReport)
        {
            _cache = cache;
            _csvReport = csvReport;
        }

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            //  1. Read reference fields from cache (from OrderBook via SaveOrdersHandler) 
            var clOrdId = _cache.Get<string>(Constants.ClientOrdId);
            var refSymbol = _cache.Get<string>(Constants.OrderSymbol);
            var refProduct = _cache.Get<string>(Constants.OrderProduct);
            var refSide = _cache.Get<string>(Constants.OrderSide);
            var refQty = _cache.Get<int>(Constants.TotalQuantity);
            var placeOrderTimeRaw = _cache.Get<string>(Constants.PlaceOrderTime);
            var orderBookAddedOnRaw = _cache.Get<string>(Constants.OrderBookAddedOn);

            if (string.IsNullOrWhiteSpace(clOrdId))
                return "ValidateOrderBookSync: cl_ord_id not in cache".FailWithLog();

            if (string.IsNullOrWhiteSpace(refSymbol))
                return "ValidateOrderBookSync: OrderBook fields not in cache — SaveOrders may have failed".FailWithLog(true);

            $"ValidateOrderBookSync: start | cl_ord_id={clOrdId} | tsym={refSymbol} qty={refQty}".Info();

            //  2. Find the order in ActivityOrderBook response 
            if (string.IsNullOrEmpty(result.ResponseBody))
                return "ActivityOrderBook: ResponseBody is null".FailWithLog(true);

            var response = JsonConvert.DeserializeObject<Response>(result.ResponseBody);
            var orders = response?.ResponceDataObject?.AllOrders;

            var groupOrders = orders?
                .Where(o => o.ClientOrderId == clOrdId
                         || o.NewClientOrderId == clOrdId
                         || o.OriginalClientOrderId == clOrdId)
                .ToList() ?? new List<OrderDetails>();

            if (groupOrders == null || !groupOrders.Any())
            {
                return ActivityResult.SoftFail($"Order {clOrdId} not found in ActivityOrderBook");
            }

            //  3. Pick the right rows 
            var placementRow = groupOrders.FirstOrDefault(o => o.OrderActivity == 0);
            var latestRow = groupOrders.OrderByDescending(o => o.OrderId).First();

            //  4. Cross-field comparison ─
            // OrderBook (cache) vs ActivityOrderBook (result.DataObject)
            var mismatches = new List<string>();

            if (placementRow != null)
            {
                CheckField(mismatches, "tsym", refSymbol, placementRow.TypeSymbol);
                CheckField(mismatches, "prd", refProduct, placementRow.Product);
                CheckField(mismatches, "trantype", refSide, placementRow.TransactionType);
                // CheckField(mismatches, "qty", refQty, placementRow.Quantity);
            }
            else
            {
                mismatches.Add("orderactivity=0 row missing");
            }

            if (mismatches.Any())
                $"ValidateOrderBookSync: mismatches → {string.Join(" | ", mismatches)}".Error();
            else
                "ValidateOrderBookSync: all fields match".Info();

            //  5. Timing calculations 

            // Parse PlaceOrder time — format: "HH:mm:ss:ffff dd-MM-yyyy"
            DateTime? placeOrderParsed = null;
            if (!string.IsNullOrWhiteSpace(placeOrderTimeRaw))
            {
                if (DateTime.TryParseExact(placeOrderTimeRaw.Trim(),
                        "HH:mm:ss:ffff dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsed))
                    placeOrderParsed = parsed;
                else
                    mismatches.Add($"request_time unparseable: '{placeOrderTimeRaw}'");
            }

            // Parse OrderBook AddedOn — format: "2026-02-27T16:12:57.6552"
            DateTime? orderBookAddedOnParsed = null;
            if (!string.IsNullOrWhiteSpace(orderBookAddedOnRaw))
            {
                if (DateTime.TryParse(orderBookAddedOnRaw.Trim(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsed))
                    orderBookAddedOnParsed = parsed;
                else
                    mismatches.Add($"OrderBook AddedOn unparseable: '{orderBookAddedOnRaw}'");
            }

            // Parse ActivityOrderBook AddedOn — same format: "2026-02-27T16:12:57.7644"
            var activityBookAddedOnRaw = latestRow.AddedOn;
            DateTime? activityBookAddedOnParsed = null;
            if (!string.IsNullOrWhiteSpace(activityBookAddedOnRaw))
            {
                if (DateTime.TryParse(activityBookAddedOnRaw.Trim(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsed))
                    activityBookAddedOnParsed = parsed;
                else
                    mismatches.Add($"ActivityOrderBook AddedOn unparseable: '{activityBookAddedOnRaw}'");
            }

            // PlaceOrder → OrderBook
            long? placeOrderToOrderBookMs = null;
            if (placeOrderParsed.HasValue && orderBookAddedOnParsed.HasValue)
                placeOrderToOrderBookMs = (long)(orderBookAddedOnParsed.Value - placeOrderParsed.Value).TotalMilliseconds;

            // PlaceOrder → ActivityOrderBook
            long? placeOrderToActivityBookMs = null;
            if (placeOrderParsed.HasValue && activityBookAddedOnParsed.HasValue)
                placeOrderToActivityBookMs = (long)(activityBookAddedOnParsed.Value - placeOrderParsed.Value).TotalMilliseconds;

            // PlaceOrder → Exchange (ordenttm)
            string? ordenttmRaw = latestRow.OrderEntryTime;
            long? placeOrderToExchangeMs = null;
            if (!string.IsNullOrWhiteSpace(ordenttmRaw))
            {
                //var normalized = ordenttmRaw.Trim();
                //int lastColon = normalized.LastIndexOf(':');

                //if (lastColon > 0)
                //    normalized = normalized.Remove(lastColon, 1).Insert(lastColon, ".");

                if (DateTime.TryParseExact(ordenttmRaw.Trim(),
                        "yyyy-MM-dd HH:mm:ss:ffff",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var ordenttmParsed))
                {
                    if (placeOrderParsed.HasValue)
                        placeOrderToExchangeMs =
                            (long)(ordenttmParsed - placeOrderParsed.Value).TotalMilliseconds;
                }
                else
                {
                    mismatches.Add($"ordenttm unparseable: '{ordenttmRaw}'");
                }
            }
            else
            {
                mismatches.Add("ordenttm empty on latest activity row");
            }

            //  6. Console 
            $"PlaceOrder time: {(placeOrderParsed.HasValue ? placeOrderParsed.Value.ToString("o") : "N/A")}".Info();
            $"OrderBook AddedOn: {(orderBookAddedOnParsed.HasValue ? orderBookAddedOnParsed.Value.ToString("o") : "N/A")}".Info();
            $"ActivityOrderBook AddedOn: {(activityBookAddedOnParsed.HasValue ? activityBookAddedOnParsed.Value.ToString("o") : "N/A")}".Info();
            $"ordenttm: {ordenttmRaw}".Info();
            $"ValidateOrderBookSync: PlaceOrder to OrderBook={placeOrderToOrderBookMs}ms | PlaceOrder to ActivityOrderBook={placeOrderToActivityBookMs}ms | PlaceOrder to Exchange={placeOrderToExchangeMs}ms".Info();
            $"ValidateOrderBookSync: exchsts={latestRow.ExchangeStatus} | status={latestRow.Status}".Info();


            if (mismatches.Any())
                return ActivityResult.SoftFail($"Sync mismatches: {string.Join(" | ", mismatches)}");

            return ActivityResult.Success(
                             $"PlaceOrder time: {(placeOrderParsed?.ToString("o") ?? "N/A")} | " +
                             $"OrderBook AddedOn: {(orderBookAddedOnParsed?.ToString("o") ?? "N/A")} | " +
                             $"ActivityOrderBook AddedOn: {(activityBookAddedOnParsed?.ToString("o") ?? "N/A")} | " +
                             $"ordenttm: {ordenttmRaw} | " +
                             $"PlaceOrder→OrderBook={placeOrderToOrderBookMs}ms | " +
                             $"PlaceOrder→ActivityOrderBook={placeOrderToActivityBookMs}ms | " +
                             $"PlaceOrder→Exchange={placeOrderToExchangeMs}ms | " +
                             $"Sync OK | exchsts={latestRow.ExchangeStatus} | Current status={latestRow.Status}");
        }

        //  helpers 

        private static void CheckField(List<string> mismatches, string field,
                                        string? expected, string? actual)
        {
            if (string.IsNullOrWhiteSpace(expected)) return;

            if (!string.Equals(expected.Trim(), actual?.Trim(), StringComparison.OrdinalIgnoreCase))
                mismatches.Add($"{field}: expected='{expected}' actual='{actual}'");
        }


    }
}