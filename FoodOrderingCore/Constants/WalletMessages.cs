namespace FoodOrderingCore.Constants
{
    public static class WalletMessages
    {
        // Balance
        public const string GetBalanceSuccess = "Wallet balance retrieved successfully";
        public const string SufficientBalance = "Sufficient balance";
        public const string InsufficientBalance = "Insufficient balance";
        public const string Unauthorized = "User is not authenticated";

        // Deposit
        public const string DepositRequestSuccess = "Please complete payment via MoMo";
        public const string DepositRequestFailed = "Failed to create deposit request";
        public const string DepositSuccess = "Deposit completed successfully";
        public const string DepositFailed = "Deposit failed";

        // IPN
        public const string InvalidSignature = "Invalid signature";
        public const string InvalidTransactionType = "Invalid transaction type";
        public const string InvalidOrderId = "Invalid order ID";

        // Transaction History
        public const string GetTransactionsSuccess = "Transaction history retrieved successfully";

        // Errors
        public const string UserNotFound = "User not found";
        public const string ProcessingError = "An error occurred while processing";
    }
}
