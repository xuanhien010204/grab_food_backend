using System.ComponentModel.DataAnnotations;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Request
{
    /// <summary>
    /// Request to create a new order
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Store ID
        /// </summary>
        [Required(ErrorMessage = "StoreId is required")]
        public long StoreId { get; set; }

        /// <summary>
        /// Payment method (1=Wallet, 2=COD, 3=MoMo)
        /// </summary>
        [Required(ErrorMessage = "PaymentMethod is required")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Wallet;

        /// <summary>
        /// Delivery address
        /// </summary>
        [Required(ErrorMessage = "DeliveryAddress is required")]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Recipient phone number
        /// </summary>
        [Required(ErrorMessage = "RecipientPhone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string RecipientPhone { get; set; }

        /// <summary>
        /// Recipient name
        /// </summary>
        [Required(ErrorMessage = "RecipientName is required")]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        /// <summary>
        /// Order note (optional)
        /// </summary>
        [MaxLength(500)]
        public string Note { get; set; }

        /// <summary>
        /// Delivery fee
        /// </summary>
        public decimal DeliveryFee { get; set; } = 0;

        /// <summary>
        /// Discount amount (from voucher)
        /// </summary>
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// Order items
        /// </summary>
        [Required(ErrorMessage = "Items are required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<OrderItemRequest> Items { get; set; }
    }

    /// <summary>
    /// Order item detail
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>
        /// FoodStore ID (Food + Store + Size combination)
        /// </summary>
        [Required]
        public Guid FoodStoreId { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Required]
        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request to update order status
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// New status
        /// </summary>
        [Required]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Reason (required for cancellation)
        /// </summary>
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}
