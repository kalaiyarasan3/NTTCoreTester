using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;


namespace NTTCoreTester.Activities
{
    public class ExtractPrePositions(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractPrePositions);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var positionsArray = result.DataObject?["Positions"] as JArray;

                if (positionsArray == null)
                    return ActivityResult.SoftFail("No positions yet.");

                var positions = positionsArray
                    .Select(p => new PositionBookModel
                    {
                        Symbol = p["tsym"]?.Value<string>() ?? "",
                        ProductType = p["prd"]?.Value<string>() ?? "",
                        NetQty = p["netqty"]?.Value<int>() ?? 0,
                        NetAvgPrice = p["netavgprc"]?.Value<decimal>() ?? 0
                    }).Where(p => p.NetQty != 0)
                    .ToList();

                _cache.Set(Constants.PrePositions, positions);

                var log = string.Join(" || ", positions.Select(p =>
                $"{p.Symbol}-{p.ProductType} Qty:{p.NetQty} Avg:{p.NetAvgPrice}"));

                log.Warn();

                return ActivityResult.Success($"Positions Extracted and Stored{log}");
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error extracting pre positions: {ex.Message}");
            }
        }
    }

}
