using System.Security.Cryptography;
using System.Text;

namespace FoodOrderingCore.Helpers
{
    // Helper class for MoMo payment operations
    public static class MomoPaymentHelper
    {
        // Compute HMAC SHA256 signature according to MoMo specification
        // </summary>
        // <param name="message">The raw signature string to sign</param>
        // <param name="secretKey">MoMo secret key</param>
        // <returns>Hex-encoded HMAC SHA256 signature</returns>
        public static string ComputeHmacSha256(string message, string secretKey)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey));

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // Build raw signature string for Create Payment request
        // Format: accessKey=xxx&amount=xxx&extraData=xxx&ipnUrl=xxx&orderId=xxx&orderInfo=xxx&partnerCode=xxx&redirectUrl=xxx&requestId=xxx&requestType=xxx
        public static string BuildCreatePaymentRawSignature(
            string accessKey,
            long amount,
            string extraData,
            string ipnUrl,
            string orderId,
            string orderInfo,
            string partnerCode,
            string redirectUrl,
            string requestId,
            string requestType)
        {
            return $"accessKey={accessKey}" +
                   $"&amount={amount}" +
                   $"&extraData={extraData}" +
                   $"&ipnUrl={ipnUrl}" +
                   $"&orderId={orderId}" +
                   $"&orderInfo={orderInfo}" +
                   $"&partnerCode={partnerCode}" +
                   $"&redirectUrl={redirectUrl}" +
                   $"&requestId={requestId}" +
                   $"&requestType={requestType}";
        }

        // Build raw signature string for IPN verification
        // Format: accessKey=xxx&amount=xxx&extraData=xxx&message=xxx&orderId=xxx&orderInfo=xxx&orderType=xxx&partnerCode=xxx&payType=xxx&requestId=xxx&responseTime=xxx&resultCode=xxx&transId=xxx
        public static string BuildIpnRawSignature(
            string accessKey,
            long amount,
            string extraData,
            string message,
            string orderId,
            string orderInfo,
            string orderType,
            string partnerCode,
            string payType,
            string requestId,
            long responseTime,
            int resultCode,
            long transId)
        {
            return $"accessKey={accessKey}" +
                   $"&amount={amount}" +
                   $"&extraData={extraData}" +
                   $"&message={message}" +
                   $"&orderId={orderId}" +
                   $"&orderInfo={orderInfo}" +
                   $"&orderType={orderType}" +
                   $"&partnerCode={partnerCode}" +
                   $"&payType={payType}" +
                   $"&requestId={requestId}" +
                   $"&responseTime={responseTime}" +
                   $"&resultCode={resultCode}" +
                   $"&transId={transId}";
        }

        // Verify IPN signature from MoMo
        // <param name="receivedSignature">Signature received from MoMo</param>
        // <param name="computedSignature">Signature computed by server</param>
        // <returns>True if signatures match</returns>
        public static bool VerifySignature(string receivedSignature, string computedSignature)
        {
            if (string.IsNullOrEmpty(receivedSignature) || string.IsNullOrEmpty(computedSignature))
                return false;

            return string.Equals(receivedSignature, computedSignature, StringComparison.OrdinalIgnoreCase);
        }

        // Generate unique request ID
        public static string GenerateRequestId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
