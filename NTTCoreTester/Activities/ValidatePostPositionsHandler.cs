using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ValidatePostPositionsHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ValidatePostPositions";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var postArray = result.DataObject?["Positions"] as JArray;

                if (postArray == null)
                    return ActivityResult.HardFail("Post positions not found");

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                if (prePositions == null)
                    return ActivityResult.HardFail("Pre positions missing");

                string? symbol = _cache.Get<string>(Constants.OrderSymbol);
                string? product = _cache.Get<string>(Constants.OrderProduct);
                string? transactionType = _cache.Get<string>(Constants.OrderSide);
                int filledQty = _cache.Get<int>(Constants.FilledQty);

                var postRow = postArray
                    .FirstOrDefault(p =>
                        p["tsym"]?.Value<string>() == symbol &&
                        p["prd"]?.Value<string>() == product);

                int postQty = postRow?["netqty"]?.Value<int>() ?? 0;

                var preRow = prePositions
                    .FirstOrDefault(p =>
                        p.Symbol == symbol &&
                        p.ProductType == product);

                int preQty = preRow?.NetQty ?? 0;

                bool isBuy = string.Equals(transactionType?.Trim(), "Buy", StringComparison.OrdinalIgnoreCase);
                int expectedQty = isBuy ? preQty + filledQty : preQty - filledQty;

                if (postQty != expectedQty)
                {
                    return ActivityResult.SoftFail(
                        $"Position mismatch. Expected: {expectedQty}, Actual: {postQty}");
                }

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error validating post positions: {ex.Message}");
            }
        }
    }
}