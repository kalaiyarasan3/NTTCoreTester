using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ValidateSquareOffExecution(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ValidateSquareOffExecution);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                var postPositions = _cache.Get<List<PositionBookModel>>(Constants.PostPositions);

                if (prePositions == null || postPositions == null)
                    return "Positions missing for square-off validation".FailWithLog(true);

                var ordersToken = result.DataObject?["AllOrders"];

                if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                    return "AllOrders not found".FailWithLog(true);

                var orders = ordersToken.ToObject<List<OrderDetails>>();

                var clientOrderIds = _cache.Get<Dictionary<string, string>>(Constants.SquareOffClientOrdIds);

                if (clientOrderIds == null || !clientOrderIds.Any())
                    return "ClientOrdIds not found in cache".FailWithLog(true);

                var errors = new List<string>();

                foreach (var pre in prePositions)
                {
                    if (pre.NetQty == 0)
                        continue;

                    string key = $"{pre.Symbol}-{pre.ProductType}";

                    if (!clientOrderIds.TryGetValue(key, out var clientId))
                    {
                        errors.Add($"Square-off ClientOrdId missing for {key}");
                        continue;
                    }

                    var post = postPositions
                        .FirstOrDefault(p =>
                            p.Symbol == pre.Symbol &&
                            p.ProductType == pre.ProductType);

                    int postQty = post?.NetQty ?? 0;

                    var symbolOrders = orders
                        .Where(o => o.ClientOrderId == clientId)
                        .OrderByDescending(x => x.AddedOn)
                        .ToList();

                    if (!symbolOrders.Any())
                    {
                        errors.Add($"Square-off order missing for {pre.Symbol}");
                        continue;
                    }

                    bool filled = symbolOrders.Any(o =>
                        o.OrderStatus == OrderEnumStatus.ORDER_TRADED);

                    if (!filled)
                    {
                        var states = string.Join(",", symbolOrders.Select(o => o.ExchangeStatus));
                        errors.Add(
                            $"Square-off order not filled for {pre.Symbol}. States:{states}");
                        continue;
                    }

                    if (postQty != 0)
                    {
                        errors.Add(
                            $"Square-off incomplete for {pre.Symbol}. " +
                            $"Order traded but position not closed. Pre:{pre.NetQty} Post:{postQty}");
                    }
                }

                _cache.Remove(Constants.SquareOffClientOrdIds);

                if (errors.Any())
                    return string.Join(" | ", errors).FailWithLog();

                return ActivityResult.Success("Square-off validation successful");
            }
            catch (Exception ex)
            {
                return $"Error in ValidateSquareOffExecution: {ex.Message}".FailWithLog(true);
            }
        }
    }
}