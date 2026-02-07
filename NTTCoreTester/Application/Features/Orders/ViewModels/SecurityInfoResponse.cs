using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.ViewModels
{
    public class SecurityInfoResponse
    {
        [JsonProperty("exch")]
        public string Exch { get; set; } = string.Empty;

        [JsonProperty("Segment")]
        public int Segment { get; set; }

        [JsonProperty("token")]
        public int Token { get; set; }

        [JsonProperty("SegmentId")]
        public int SegmentId { get; set; }

        [JsonProperty("tsym")]
        public string Tsym { get; set; } = string.Empty;

        [JsonProperty("symname")]
        public string Symname { get; set; } = string.Empty;

        [JsonProperty("instname")]
        public string Instname { get; set; } = string.Empty;

        [JsonProperty("pp")]
        public string Pp { get; set; } = string.Empty;

        [JsonProperty("ls")]
        public int Ls { get; set; }

        [JsonProperty("ti")]
        public decimal Ti { get; set; }

        [JsonProperty("mult")]
        public decimal Mult { get; set; }

        [JsonProperty("prcftr_d")]
        public string PrcftrD { get; set; } = string.Empty;

        [JsonProperty("trdunt")]
        public string Trdunt { get; set; } = string.Empty;

        [JsonProperty("delunt")]
        public string Delunt { get; set; } = string.Empty;

        [JsonProperty("varmrg")]
        public string Varmrg { get; set; } = string.Empty;

        [JsonProperty("weekly")]
        public string Weekly { get; set; } = string.Empty;

        [JsonProperty("cname")]
        public string Cname { get; set; } = string.Empty;

        [JsonProperty("exd")]
        public string Exd { get; set; } = string.Empty;

        [JsonProperty("strprc")]
        public decimal Strprc { get; set; }

        [JsonProperty("lp")]
        public decimal Lp { get; set; }

        [JsonProperty("c")]
        public decimal C { get; set; }

        [JsonProperty("last_trd_d")]
        public string LastTrdD { get; set; } = string.Empty;

        [JsonProperty("lc")]
        public decimal Lc { get; set; }

        [JsonProperty("uc")]
        public decimal Uc { get; set; }

        [JsonProperty("optt")]
        public string Optt { get; set; } = string.Empty;

        [JsonProperty("ISIN")]
        public string Isin { get; set; } = string.Empty;

        [JsonProperty("High")]
        public decimal High { get; set; }

        [JsonProperty("Low")]
        public decimal Low { get; set; }

        [JsonProperty("Open")]
        public decimal Open { get; set; }

        [JsonProperty("Volume")]
        public decimal Volume { get; set; }

        [JsonProperty("OpenIntrest")]
        public decimal OpenIntrest { get; set; }

        [JsonProperty("WeekHigh52")]
        public decimal WeekHigh52 { get; set; }

        [JsonProperty("WeekLow52")]
        public decimal WeekLow52 { get; set; }

        [JsonProperty("PercentangeChange")]
        public decimal PercentangeChange { get; set; }

        [JsonProperty("PriceIndicator")]
        public string PriceIndicator { get; set; } = string.Empty;

        [JsonProperty("HL52Time")]
        public int Hl52Time { get; set; }

        [JsonProperty("SCStage")]
        public string Scstage { get; set; } = string.Empty;

        [JsonProperty("SpotPrice")]
        public decimal SpotPrice { get; set; }

        [JsonProperty("IntrinsicValue")]
        public decimal IntrinsicValue { get; set; }

        [JsonProperty("MoneyType")]
        public string MoneyType { get; set; } = string.Empty;

        [JsonProperty("request_time")]
        public string RequestTime { get; set; } = string.Empty;
    }
}
