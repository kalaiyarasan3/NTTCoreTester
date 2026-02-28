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
    public class ExtractSquareOffOrderHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ExtractSquareOffOrder";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var ordersToken = result.DataObject?["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var symbol = _cache.Get<string>(Constants.OrderSymbol);
            var product = _cache.Get<string>(Constants.OrderProduct);
            var originalSide = _cache.Get<string>(Constants.OrderSide);

            // Determine square-off side (opposite)
            var squareOffSide = originalSide.Equals("Buy", StringComparison.OrdinalIgnoreCase)
                ? "Sell"
                : "Buy";

            var candidateOrders = orders?
                .Where(o => o.TypeSymbol == symbol &&
                            o.Product == product &&
                            o.TransactionType.Equals(squareOffSide, StringComparison.OrdinalIgnoreCase)) 
                .ToList();

            if (candidateOrders == null || !candidateOrders.Any())
                return "Square-off order not found".FailWithLog(true);

            var latestOrder = candidateOrders.First();

            var latestClientId = latestOrder.NewClientOrderId;

            var relatedOrders = orders
                .Where(o => o.NewClientOrderId == latestClientId)
                .ToList();

            bool isFilled = relatedOrders.Any(o => o.Status == "1118");

            _cache.Set(Constants.ClientOrdId, latestClientId);
            _cache.Set(Constants.OrderSide, latestOrder.TransactionType);

            $"Square-off Order Found: {latestClientId} qty: {latestOrder.Quantity} side: {latestOrder.TransactionType} statusFilled: {isFilled}".Warn();

            if (!isFilled)
            {
                return $"Square-off order not filled yet. Current states: {string.Join(" | ", relatedOrders.Select(o => o.Status))}"
                    .FailWithLog(false);
            }

            return ActivityResult.Success();
        }
    }
}
