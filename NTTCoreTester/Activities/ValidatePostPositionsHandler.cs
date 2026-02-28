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
                    return "Post positions not found in ValidatePostPositionsHandler".FailWithLog(true);

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);
                if (prePositions == null)
                    return "Pre positions missing in ValidatePostPositionsHandler".FailWithLog(true);

                string? symbol = _cache.Get<string>(Constants.OrderSymbol);
                string? product = _cache.Get<string>(Constants.OrderProduct);
                string? transactionType = _cache.Get<string>(Constants.OrderSide);
                int filledQty = _cache.Get<int>(Constants.FilledQty);

                var postPositions = postArray.ToObject<List<PositionBookModel>>();

                var postRow = postPositions?.FirstOrDefault(p =>
                        p.Symbol == symbol &&
                        p.ProductType == product);
                $"Position for symbol: {symbol} /product: {product}".Warn();

                if (postRow == null)
                {
                    return $"Position for symbol: {symbol} /product: {product} not found in post positions".FailWithLog(true);
                }

                int postQty = postRow.NetQty;

                var preRow = prePositions
                    .FirstOrDefault(p =>
                        p.Symbol == symbol &&
                        p.ProductType == product);

                int preQty = preRow?.NetQty ?? 0;

                $"PreQty: {preQty}, FilledQty: {filledQty}, PostQty: {postQty}".Warn();

                int expectedQty = preQty + filledQty;
                if (postQty != expectedQty)
                {
                    return $"Position mismatch. Expected: {expectedQty}, Actual: {postQty}".FailWithLog();
                }

                _cache.Set(Constants.PostPositions, postPositions);
                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error ValidatePostPositionsHandler: {ex.Message}".FailWithLog(true);
            }
        }
    }
}