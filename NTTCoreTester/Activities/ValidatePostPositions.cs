using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ValidatePostPositions(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ValidatePostPositions);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var postArray = result.DataObject?["Positions"] as JArray;

                if (postArray == null)
                    return "Post positions missing".FailWithLog(true);

                var postPositions = postArray.ToObject<List<PositionBookModel>>() ?? new();

                var prePositions = _cache.Get<List<PositionBookModel>>(Constants.PrePositions);

                if (prePositions == null)
                    return "Pre positions missing".FailWithLog(true);

                var filledQtyBySymbol = _cache.Get<Dictionary<string, int>>(Constants.FilledQtyBySymbol);

                var errors = new List<string>();
                var logs = new List<string>();

                foreach (var post in postPositions)
                {
                    string key = $"{post.Symbol}-{post.ProductType}";

                    var pre = prePositions.FirstOrDefault(p =>
                        p.Symbol == post.Symbol &&
                        p.ProductType == post.ProductType);

                    int preQty = pre?.NetQty ?? 0;
                    int postQty = post.NetQty;

                    int tradedQty = postQty - preQty;

                    //-----------------------------------------------------
                    // Validate against TradeBook
                    //-----------------------------------------------------

                    if (filledQtyBySymbol != null &&
                        filledQtyBySymbol.TryGetValue(key, out int filledQty))
                    {
                        int expectedPostQty = preQty + filledQty;

                        if (postQty != expectedPostQty)
                        {
                            errors.Add(
                                $"TradeBook mismatch for {key}. " +
                                $"Pre:{preQty} Filled:{filledQty} Expected:{expectedPostQty} Actual:{postQty}");
                        }
                    }

                    if (tradedQty != 0)
                        logs.Add($"{key} executed qty: {tradedQty}");

                    //-----------------------------------------------------
                    // New position
                    //-----------------------------------------------------

                    if (pre == null && postQty != 0)
                    {
                        logs.Add($"{key} new position created Qty:{postQty}");
                        continue;
                    }

                    //-----------------------------------------------------
                    // Unchanged
                    //-----------------------------------------------------

                    if (preQty == postQty)
                    {
                        logs.Add($"{key} unchanged");
                        continue;
                    }

                    //-----------------------------------------------------
                    // Square-off
                    //-----------------------------------------------------

                    if (preQty != 0 && postQty == 0)
                    {
                        logs.Add($"{key} fully squared-off. Pre:{preQty} Post:{postQty}");
                        continue;
                    }

                    //-----------------------------------------------------
                    // Partial close
                    //-----------------------------------------------------

                    if (Math.Sign(preQty) == Math.Sign(postQty) &&
                        Math.Abs(postQty) < Math.Abs(preQty))
                    {
                        logs.Add($"{key} partial close detected");
                        continue;
                    }

                    //-----------------------------------------------------
                    // Position increase
                    //-----------------------------------------------------

                    if (Math.Sign(preQty) == Math.Sign(postQty) &&
                        Math.Abs(postQty) > Math.Abs(preQty))
                    {
                        logs.Add($"{key} position increased");
                        continue;
                    }

                    //-----------------------------------------------------
                    // Position flip
                    //-----------------------------------------------------

                    if (Math.Sign(preQty) != Math.Sign(postQty))
                    {
                        logs.Add($"{key} position flipped");
                        continue;
                    }

                    errors.Add($"Position mismatch for {key}. Pre:{preQty} Post:{postQty}");
                }


                //---------------------------------------------------------
                // Store post positions for later activities
                //---------------------------------------------------------

                _cache.Set(Constants.PostPositions, postPositions);

                _cache.Remove(Constants.ClientOrdIds);
                _cache.Remove(Constants.FilledQtyBySymbol);

                if (errors.Any())
                    return string.Join(" | ", errors).FailWithLog(false);

                var response = string.Join(" | ", logs);
                response.Warn();
                return ActivityResult.Success(response);
            }
            catch (Exception ex)
            {
                return $"Error ValidatePostPositions: {ex.Message}".FailWithLog(true);
            }
        }
    }
}