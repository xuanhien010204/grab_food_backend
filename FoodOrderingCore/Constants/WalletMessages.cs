namespace FoodOrderingCore.Constants
{
    public static class WalletMessages
    {
        // Balance
        public const string GetBalanceSuccess = "Lấy thông tin ví thành công";
        public const string SufficientBalance = "Đủ số dư";
        public const string InsufficientBalance = "Không đủ số dư";
        
        // Deposit
        public const string DepositRequestSuccess = "Vui lòng thanh toán qua MoMo";
        public const string DepositRequestFailed = "Tạo yêu cầu nạp tiền thất bại";
        public const string DepositSuccess = "Nạp tiền thành công!";
        public const string DepositFailed = "Nạp tiền thất bại";
        
        // IPN
        public const string InvalidSignature = "Chữ ký không hợp lệ";
        public const string InvalidTransactionType = "Loại giao dịch không hợp lệ";
        public const string InvalidOrderId = "Mã đơn hàng không hợp lệ";
        
        // Transaction History
        public const string GetTransactionsSuccess = "Lấy lịch sử giao dịch thành công";
        
        // Errors
        public const string UserNotFound = "Không tìm thấy người dùng";
        public const string ProcessingError = "Có lỗi xảy ra khi xử lý";
    }
}
