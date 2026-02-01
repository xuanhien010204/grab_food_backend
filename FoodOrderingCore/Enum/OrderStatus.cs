namespace FoodOrderingCore.Enum
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Preparing = 2,
        Ready = 3,
        Delivering = 4,
        Completed = 5,
        Cancelled = 6
    }

    public enum PaymentMethod
    {
        Wallet = 1,
        CashOnDelivery = 2,
        MoMo = 3
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2,
        Failed = 3
    }
}
