using Newtonsoft.Json;

namespace NTTCoreTester.Models
{
    public class LimitMarginDetails
    {
        [JsonProperty("UsedMargin")]
        public decimal UsedMargin { get; set; }

        [JsonProperty("NetPremium")]
        public decimal NetPremium { get; set; }

        [JsonProperty("UsedMarginWithoutPL")]
        public decimal UsedMarginWithoutPL { get; set; }

        [JsonProperty("ReamainingMargin")]
        public decimal RemainingMargin { get; set; }

        [JsonProperty("RemainingMarginForDisplay")]
        public decimal RemainingMarginForDisplay { get; set; }

        [JsonProperty("AvailableMarginPercentage")]
        public decimal AvailableMarginPercentage { get; set; }

        [JsonProperty("UsedMarginPercentage")]
        public decimal UsedMarginPercentage { get; set; }

        [JsonProperty("CashUsed")]
        public decimal CashUsed { get; set; }

        [JsonProperty("TotalCash")]
        public decimal TotalCash { get; set; }

        [JsonProperty("Charges")]
        public decimal Charges { get; set; }

        [JsonProperty("Transfer_Funds_Received")]
        public decimal TransferFundsReceived { get; set; }

        [JsonProperty("UsedMarginWithoutCharges")]
        public decimal UsedMarginWithoutCharges { get; set; }

        [JsonProperty("UsedMarginPercentageWithoutCharges")]
        public decimal UsedMarginPercentageWithoutCharges { get; set; }

        [JsonProperty("TransferableAmount")]
        public decimal TransferableAmount { get; set; }

        [JsonProperty("WithdrawableAmount")]
        public decimal WithdrawableAmount { get; set; }
    }
}