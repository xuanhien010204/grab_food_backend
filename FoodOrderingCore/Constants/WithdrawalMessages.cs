namespace FoodOrderingCore.Constants
{
    public static class WithdrawalMessages
    {
        public const string CreateSuccess = "Yêu cầu rút tiền đã được tạo thành công";
        public const string ApproveSuccess = "Yêu cầu rút tiền đã được duyệt";
        public const string RejectSuccess = "Yêu cầu rút tiền đã bị từ chối";
        public const string GetSuccess = "Lấy danh sách yêu cầu rút tiền thành công";
        public const string NotFound = "Không tìm thấy yêu cầu rút tiền";
        public const string InsufficientBalance = "Số dư ví không đủ để rút";
        public const string AlreadyProcessed = "Yêu cầu rút tiền này đã được xử lý";
        public const string Unauthorized = "Bạn không có quyền thực hiện thao tác này";
        public const string OnlyManagerCanWithdraw = "Chỉ manager mới có thể tạo yêu cầu rút tiền";
        public const string OnlyAdminCanProcess = "Chỉ admin mới có thể xử lý yêu cầu rút tiền";
    }
}
