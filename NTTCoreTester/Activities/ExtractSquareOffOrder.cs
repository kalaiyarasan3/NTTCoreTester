using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractSquareOffOrder(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractSquareOffOrder);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var ordersToken = result.DataObject?["AllOrders"];

                if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                    return "AllOrders not found".FailWithLog(true);

                var orders = ordersToken.ToObject<List<OrderDetails>>();

                if (orders == null || !orders.Any())
                    return "Order list empty".FailWithLog(true);

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);

                if (prePositions == null || !prePositions.Any())
                    return "PrePositions missing".FailWithLog(true);

                var clientOrderIds = new Dictionary<string, string>();

                var errors = new List<string>();

                foreach (var pos in prePositions)
                {
                    if (pos.NetQty == 0)
                        continue;

                    string expectedSide = pos.NetQty > 0 ? "Sell" : "Buy";

                    var matchingOrders = orders
                        .Where(o =>
                            o.TypeSymbol == pos.Symbol &&
                            o.Product == pos.ProductType &&
                            o.TransactionType.Equals(expectedSide, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.AddedOn)
                        .ToList();

                    if (!matchingOrders.Any())
                    {
                        errors.Add($"Square-off order not found for {pos.Symbol}-{pos.ProductType}");
                        continue;
                    }

                    var latestOrder = matchingOrders.First();

                    string key = $"{pos.Symbol}-{pos.ProductType}";
                    string clientOrdId = latestOrder.NewClientOrderId;

                    clientOrderIds[key] = clientOrdId;
                }

                if (errors.Any())
                    return string.Join(" | ", errors).FailWithLog(true);

                _cache.Set(Constants.ClientOrdIds, clientOrderIds);
                _cache.Set(Constants.SquareOffClientOrdIds, clientOrderIds);

                $"Square-off orders: {string.Join(", ", clientOrderIds.Select(x => $"{x.Key}:{x.Value}"))}".Warn();

                return ActivityResult.Success("Square-off orders extracted successfully");
            }
            catch (Exception ex)
            {
                return $"Error ExtractSquareOffOrder: {ex.Message}".FailWithLog(true);
            }
        }
    }
}