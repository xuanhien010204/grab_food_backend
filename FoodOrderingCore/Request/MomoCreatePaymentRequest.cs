using Newtonsoft.Json;

namespace FoodOrderingCore.Request
{
    // Based on MoMo API v2 specification
    public class MomoCreatePaymentRequest
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("partnerName")]
        public string PartnerName { get; set; } = "FoodOrdering";

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderInfo")]
        public string OrderInfo { get; set; }

        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty("ipnUrl")]
        public string IpnUrl { get; set; }

        [JsonProperty("requestType")]
        public string RequestType { get; set; } = "captureWallet";

        [JsonProperty("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonProperty("lang")]
        public string Lang { get; set; } = "vi";

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("autoCapture")]
        public bool AutoCapture { get; set; } = true;
    }
}
