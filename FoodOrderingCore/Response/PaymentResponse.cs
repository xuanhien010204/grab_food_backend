namespace FoodOrderingCore.Response
{
    public class PaymentResponse
    {
        public string OrderId { get; set; }

        public long Amount { get; set; }

        /// URL to redirect user to MoMo payment page (Web)
        public string PayUrl { get; set; }

        // Deep link to open MoMo app directly (Mobile)
        public string DeepLink { get; set; }

        // QR Code URL for scanning
        public string QrCodeUrl { get; set; }

        // Response message from MoMo
        public string Message { get; set; }

        public bool Success { get; set; }
    }
}
