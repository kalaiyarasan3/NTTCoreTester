using Newtonsoft.Json;

namespace NTTCoreTester.Models
{
    public class HoldingDetails
    {
        [JsonProperty("ExchangeData")]
        public ExchangeData ExchangeData { get; set; }

        [JsonProperty("holdqty")]
        public int HoldQuantity { get; set; }

        // NON-MTF
        [JsonProperty("PledgedQuantity")]
        public int PledgedQuantity { get; set; }

        [JsonProperty("colqty")]
        public int CollateralQuantity { get; set; }

        [JsonProperty("unplgdqty")]
        public int UnpledgedQuantity { get; set; }

        [JsonProperty("NonMTFCollateral")]
        public decimal NonMTFCollateral { get; set; }

        // MTF
        [JsonProperty("MTFpledgeQuantity")]
        public int MTFPledgeQuantity { get; set; }

        [JsonProperty("MTFcollateralQuantity")]
        public int MTFCollateralQuantity { get; set; }

        [JsonProperty("MTFunpledgeQuantity")]
        public int MTFUnpledgeQuantity { get; set; }

        [JsonProperty("MTFCollateral")]
        public decimal MTFCollateral { get; set; }
    }

    public class ExchangeData
    {
        [JsonProperty("tsym")]
        public string Symbol { get; set; }
    }
}