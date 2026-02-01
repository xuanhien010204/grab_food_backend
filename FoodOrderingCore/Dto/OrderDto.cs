using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Dto
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StoreImage { get; set; }

        public DateTime PurchaseDate { get; set; }

        // Status
        public OrderStatus Status { get; set; }
        public string StatusName => Status.ToString();

        // Payment
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName => PaymentMethod.ToString();
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusName => PaymentStatus.ToString();

        // Amounts
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        // Delivery info
        public string DeliveryAddress { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientName { get; set; }
        public string Note { get; set; }

        // Timestamps
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelReason { get; set; }

        // Items count
        public int TotalItems { get; set; }

        // Order details (for detail view)
        public List<OrderDetailDto> Items { get; set; }
    }
}
