using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Data
{
    public class Order
    {
        public Guid Id { set; get; }
        public long UserId { set; get; }
        public long StoreId { get; set; }
        public DateTime PurchaseDate { get; set; }

        // status (Pending, Confirmed, Preparing)
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Payment method (Wallet, COD, MoMo)
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Wallet;

        // Payment status (Unpaid, Paid, Refunded)
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // Subtotal before discount
        
        [Column(TypeName = "money")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "money")]
        public decimal DeliveryFee { get; set; } = 0;

        // Discount amount (from voucher)
        [Column(TypeName = "money")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        // Final total = SubTotal + DeliveryFee - Discount
        [Column(TypeName = "money")]
        public decimal Total { set; get; }

        // Delivery address
        [Column(TypeName = "nvarchar(500)")]
        public string DeliveryAddress { get; set; }

        // Recipient phone number
        [Column(TypeName = "varchar(15)")]
        public string RecipientPhone { get; set; }

        // Recipient name
        [Column(TypeName = "nvarchar(100)")]
        public string RecipientName { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Note { get; set; }

        // Reason for cancellation (if cancelled)
        [Column(TypeName = "nvarchar(500)")]
        public string CancelReason { get; set; }

        // Voucher code used (if any)
        [Column(TypeName = "varchar(50)")]
        public string VoucherCode { get; set; }

        // Time when order was confirmed
        
        public DateTime? ConfirmedAt { get; set; }

        // Time when order was completed/delivered
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public User User { set; get; }
        public Store Store { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Review Review { get; set; }
        public VoucherUsage VoucherUsage { get; set; }
    }
}
