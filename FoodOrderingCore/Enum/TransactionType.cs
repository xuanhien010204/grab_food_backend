namespace FoodOrderingCore.Enum
{
    public enum TransactionType
    {
        Deposit = 1,
        Payment = 2,
        Refund = 3,
        Withdrawal = 4,
        Bonus = 5
    }
    public enum TransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3
    }
}
