namespace FoodOrderingCore.Constants
{
    /// <summary>
    /// Response messages for Order operations
    /// </summary>
    public static class OrderMessages
    {
        // Success
        public const string CreateSuccess = "Order created successfully";
        public const string GetOrderSuccess = "Order retrieved successfully";
        public const string GetOrdersSuccess = "Orders retrieved successfully";
        public const string UpdateStatusSuccess = "Order status updated successfully";
        public const string CancelSuccess = "Order cancelled successfully";

        // Errors
        public const string OrderNotFound = "Order not found";
        public const string StoreNotFound = "Store not found";
        public const string FoodStoreNotFound = "Food item not found";
        public const string InsufficientBalance = "Insufficient wallet balance";
        public const string InvalidStatus = "Invalid order status";
        public const string CannotCancel = "Cannot cancel order in current status";
        public const string CancelReasonRequired = "Cancellation reason is required";
        public const string EmptyCart = "Cart is empty";
        public const string ItemsFromDifferentStores = "All items must be from the same store";
        public const string PaymentFailed = "Payment failed";

        // Status messages
        public const string StatusPending = "Pending";
        public const string StatusConfirmed = "Confirmed";
        public const string StatusPreparing = "Preparing";
        public const string StatusReady = "Ready for delivery";
        public const string StatusDelivering = "Out for delivery";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
    }
}
