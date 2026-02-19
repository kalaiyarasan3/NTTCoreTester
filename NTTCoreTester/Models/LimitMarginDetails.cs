using System.Text.Json.Serialization;

namespace NTTCoreTester.Models
{
    public class LimitMarginDetails
    {
        [JsonPropertyName("UsedMargin")]
        public decimal UsedMargin { get; set; }

        [JsonPropertyName("NetPremium")]
        public decimal NetPremium { get; set; }

        [JsonPropertyName("UsedMarginWithoutPL")]
        public decimal UsedMarginWithoutPL { get; set; }

        [JsonPropertyName("ReamainingMargin")]
        public decimal RemainingMargin { get; set; }

        [JsonPropertyName("RemainingMarginForDisplay")]
        public decimal RemainingMarginForDisplay { get; set; }

        [JsonPropertyName("AvailableMarginPercentage")]
        public decimal AvailableMarginPercentage { get; set; }

        [JsonPropertyName("UsedMarginPercentage")]
        public decimal UsedMarginPercentage { get; set; }

        [JsonPropertyName("CashUsed")]
        public decimal CashUsed { get; set; }

        [JsonPropertyName("TotalCash")]
        public decimal TotalCash { get; set; }

        [JsonPropertyName("Charges")]
        public decimal Charges { get; set; }

        [JsonPropertyName("Transfer_Funds_Received")]
        public decimal TransferFundsReceived { get; set; }

        [JsonPropertyName("UsedMarginWithoutCharges")]
        public decimal UsedMarginWithoutCharges { get; set; }

        [JsonPropertyName("UsedMarginPercentageWithoutCharges")]
        public decimal UsedMarginPercentageWithoutCharges { get; set; }

        [JsonPropertyName("TransferableAmount")]
        public decimal TransferableAmount { get; set; }

        [JsonPropertyName("WithdrawableAmount")]
        public decimal WithdrawableAmount { get; set; }
    }
}
