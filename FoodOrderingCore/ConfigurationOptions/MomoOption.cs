namespace FoodOrderingCore.ConfigurationOptions
{
    public class MomoOption
    {
        public string PartnerCode { get; set; }

        public string AccessKey { get; set; }

        // MoMo Secret Key for HMAC SHA256 signature
        public string SecretKey { get; set; }

        // MoMo API Endpoint
        public string ApiEndpoint { get; set; }

        // IPN (Instant Payment Notification) URL - webhook endpoint
        public string NotifyUrl { get; set; }

        // Return URL after payment completion
        public string ReturnUrl { get; set; }
    }
}