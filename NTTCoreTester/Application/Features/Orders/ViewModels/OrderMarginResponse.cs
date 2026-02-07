using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.ViewModels
{
    public class OrderMarginResponse
    {
        [JsonProperty("Remarks")]
        public string Remarks { get; set; } = null;

        [JsonProperty("cash")]
        public decimal Cash { get; set; }  

        [JsonProperty("marginused")]
        public decimal MarginUsed { get; set; }  

        [JsonProperty("ordermargin")]
        public decimal OrderMargin { get; set; }

        [JsonProperty("marginusedprev")]
        public decimal MarginUsedPrev { get; set; }

        [JsonProperty("charges")]
        public decimal Charges { get; set; }

        [JsonProperty("chargesdetail")]
        public string ChargesDetail { get; set; } = string.Empty;

        [JsonProperty("MTFMarginPercentage")]
        public decimal MtfMarginPercentage { get; set; }

        [JsonProperty("DeliveryMargin")]
        public decimal DeliveryMargin { get; set; }

        [JsonProperty("ExposureMargin")]
        public decimal ExposureMargin { get; set; }

        [JsonProperty("AdditionalMargin")]
        public decimal AdditionalMargin { get; set; }

        [JsonProperty("SpanMargin")]
        public decimal SpanMargin { get; set; }

        [JsonProperty("AvailableMargin")]
        public decimal AvailableMargin { get; set; }

        [JsonProperty("NetPremium")]
        public decimal NetPremium { get; set; }

        [JsonProperty("IsAccess")]
        public bool IsAccess { get; set; }
    }
}
