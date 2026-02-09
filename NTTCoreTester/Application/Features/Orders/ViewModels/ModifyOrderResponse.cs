using Newtonsoft.Json;

namespace NTTCoreTester.Application.Features.Orders.ViewModels
{
    public class ModifyOrderResponse
    {
        [JsonProperty("cl_ord_id")]
        public string ClOrdId { get; set; } = string.Empty;

        [JsonProperty("OrderRequestId")]
        public string? OrderRequestId { get; set; }
    }
}
