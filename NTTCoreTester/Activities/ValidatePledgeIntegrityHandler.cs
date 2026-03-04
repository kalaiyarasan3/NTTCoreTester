using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class ValidatePledgeIntegrityHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ValidatePledgeIntegrityHandler);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var holdings = result.DataObject?["Holdings"];

                if (holdings == null || holdings.Type != JTokenType.Array)
                    return "Holdings not found".FailWithLog();

                var symbol = cache.Get<string>(Constants.OrderSymbol);

                var holding = holdings
                    .FirstOrDefault(x => x["ExchangeData"]?["tsym"]?.ToString() == symbol);

                if (holding == null)
                    return $"Holding for {symbol} not found".FailWithLog();

                var errors = new List<string>();

                // ---------------------------
                // NON-MTF
                // ---------------------------

                int pledged = holding["PledgedQuantity"]?.Value<int>() ?? 0;
                int collateralQty = holding["colqty"]?.Value<int>() ?? 0;
                int unpledged = holding["unplgdqty"]?.Value<int>() ?? 0;

                decimal nonMtfCollateral = holding["NonMTFCollateral"]?.Value<decimal>() ?? 0;

                if (unpledged > pledged)
                {
                    errors.Add($"NON-MTF unpledge exceeds pledged. Pledged: {pledged}, Unpledged: {unpledged}");
                }

                if (collateralQty > pledged)
                {
                    errors.Add($"NON-MTF collateral exceeds pledged. Pledged: {pledged}, Collateral: {collateralQty}");
                }

                if (collateralQty == 0 && nonMtfCollateral > 0)
                {
                    errors.Add($"NON-MTF collateral value exists but quantity is zero.");
                }

                // ---------------------------
                // MTF
                // ---------------------------

                int mtfPledged = holding["MTFpledgeQuantity"]?.Value<int>() ?? 0;
                int mtfCollateralQty = holding["MTFcollateralQuantity"]?.Value<int>() ?? 0;
                int mtfUnpledged = holding["MTFunpledgeQuantity"]?.Value<int>() ?? 0;

                decimal mtfCollateral = holding["MTFCollateral"]?.Value<decimal>() ?? 0;

                if (mtfUnpledged > mtfCollateralQty)
                {
                    errors.Add(
                        $"MTF unpledge exceeds collateral. CollateralQty: {mtfCollateralQty}, Unpledged: {mtfUnpledged}");
                }

                if (mtfCollateralQty > mtfPledged)
                {
                    errors.Add(
                        $"MTF collateral exceeds pledged. Pledged: {mtfPledged}, Collateral: {mtfCollateralQty}");
                }

                if (mtfCollateralQty == 0 && mtfCollateral > 0)
                {
                    errors.Add(
                        $"MTF collateral value exists but collateral quantity is zero.");
                }

                if (errors.Any())
                {
                    var message = string.Join(" | ", errors);
                    return message.FailWithLog(true);
                }

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return $"Error in ValidatePledgeIntegrityHandler: {ex.Message}"
                    .FailWithLog();
            }
        }
    }
}