namespace FoodOrderingCore.Constants
{
    public static class DeliveryAddressMessages
    {
        public const string GetSuccess = "Addresses retrieved successfully";
        public const string CreateSuccess = "Address created successfully";
        public const string UpdateSuccess = "Address updated successfully";
        public const string DeleteSuccess = "Address deleted successfully";
        public const string SetDefaultSuccess = "Default address set successfully";
        public const string NotFound = "Address not found";
        public const string MaxAddressReached = "Maximum number of addresses reached";
    }

    public static class ReviewMessages
    {
        public const string CreateSuccess = "Review submitted successfully";
        public const string GetSuccess = "Reviews retrieved successfully";
        public const string ReplySuccess = "Reply submitted successfully";
        public const string DeleteSuccess = "Review deleted successfully";
        public const string NotFound = "Review not found";
        public const string AlreadyReviewed = "You have already reviewed this order";
        public const string OrderNotCompleted = "Only completed orders can be reviewed";
        public const string NotYourOrder = "You are not authorized to review this order";
    }

    public static class VoucherMessages
    {
        public const string CreateSuccess = "Voucher created successfully";
        public const string GetSuccess = "Voucher retrieved successfully";
        public const string UpdateSuccess = "Voucher updated successfully";
        public const string ApplySuccess = "Voucher applied successfully";
        public const string DeactivateSuccess = "Voucher deactivated successfully";
        public const string NotFound = "Voucher not found";
        public const string Expired = "Voucher has expired";
        public const string NotStarted = "Voucher is not yet active";
        public const string UsageLimitReached = "Voucher usage limit reached";
        public const string AlreadyUsed = "You have already used this voucher";
        public const string MinOrderNotMet = "Minimum order amount not met";
        public const string StoreNotMatch = "Voucher is not valid for this store";
        public const string CodeExists = "Voucher code already exists";
    }

    public static class FavoriteMessages
    {
        public const string AddSuccess = "Added to favorites successfully";
        public const string RemoveSuccess = "Removed from favorites successfully";
        public const string GetSuccess = "Favorites retrieved successfully";
        public const string AlreadyFavorited = "Already in favorites";
        public const string NotFound = "Not found in favorites";
    }

    public static class NotificationMessages
    {
        public const string GetSuccess = "Notifications retrieved successfully";
        public const string MarkReadSuccess = "Notification marked as read";
        public const string MarkAllReadSuccess = "All notifications marked as read";
        public const string DeleteSuccess = "Notification deleted successfully";
        public const string NotFound = "Notification not found";
    }
}
