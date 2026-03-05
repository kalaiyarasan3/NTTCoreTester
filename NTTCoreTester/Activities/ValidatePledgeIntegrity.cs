using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models; 

namespace NTTCoreTester.Activities
{
    public class ValidatePledgeIntegrity(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ValidatePledgeIntegrity);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
               
                var holdings = GetHoldings(result);

                if (holdings == null || !holdings.Any())
                    return "Holdings not found".FailWithLog();

                var symbol = cache.Get<string>(Constants.PledgeOrderSymbol);

                var holding = holdings.FirstOrDefault(x => x.ExchangeData.Symbol == "TATACONSUM-EQ");

                if (holding == null)
                    return $"Holding for {symbol} not found".FailWithLog();

                var errors = new List<string>();

                // ---------------------------
                // NON-MTF
                // ---------------------------

                int pledged = holding.PledgedQuantity;
                int collateralQty = holding.CollateralQuantity;
                int unpledged = holding.UnpledgedQuantity;

                decimal nonMtfCollateral = holding.NonMTFCollateral;


                // Unpledge cannot exceed collateral
                if (unpledged > collateralQty)
                {
                    errors.Add(
                        $"NON-MTF unpledge exceeds collateral. Collateral: {collateralQty}, Unpledged: {unpledged}");
                }

                // Collateral value consistency
                if (collateralQty == 0 && nonMtfCollateral > 0)
                {
                    errors.Add(
                        $"NON-MTF collateral value exists but quantity is zero.");
                }

                // ---------------------------
                // MTF
                // ---------------------------

                int mtfPledged = holding.MTFPledgeQuantity;
                int mtfCollateralQty = holding.MTFCollateralQuantity;
                int mtfUnpledged = holding.MTFUnpledgeQuantity;

                decimal mtfCollateral = holding.MTFCollateral;

                if (mtfUnpledged > mtfCollateralQty)
                {
                    errors.Add(
                        $"MTF unpledge exceeds collateral. CollateralQty: {mtfCollateralQty}, Unpledged: {mtfUnpledged}");
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

        private List<HoldingDetails>? GetHoldings(ApiExecutionResult result)
        {
            var holdingsArray = result.DataObject?["Holdings"];

            if (holdingsArray == null || holdingsArray.Type != JTokenType.Array)
                return null;

            return holdingsArray.ToObject<List<HoldingDetails>>();
        }
    }
}