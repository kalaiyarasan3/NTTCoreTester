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
    public class ExtractPrePositionsHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ExtractPrePositions";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var positionsArray = result.DataObject?["Positions"] as JArray;

                if (positionsArray == null)
                    return ActivityResult.HardFail("Positions not found");

                var positions = positionsArray
                    .Select(p => new PositionBookModel
                    {
                        Symbol = p["tsym"]?.Value<string>() ?? "",
                        ProductType = p["prd"]?.Value<string>() ?? "",
                        NetQty = p["netqty"]?.Value<int>() ?? 0,
                        NetAvgPrice = p["netavgprc"]?.Value<decimal>() ?? 0
                    })
                    .ToList();

                _cache.Set(Constants.PrePositions, positions);

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error extracting pre positions: {ex.Message}");
            }
        }
    }

}
