using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Data
{
    public class Voucher
    {
        public Guid Id { get; set; }
        
        // Voucher code (unique)
        [Column(TypeName = "varchar(50)")]
        public string Code { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }
        // Voucher type (Percent or FixedAmount)
        public VoucherType Type { get; set; }
        
        // Discount value (percentage or fixed amount)
        [Column(TypeName = "money")]
        public decimal Value { get; set; }
        [Column(TypeName = "money")]
        public decimal MinOrderAmount { get; set; } = 0;
        [Column(TypeName = "money")]
        public decimal? MaxDiscount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Total usage limit (null = unlimited)
        public int? UsageLimit { get; set; }
        public int? UsageLimitPerUser { get; set; } = 1;
        public int UsedCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
            // Store ID if store-specific voucher (null = platform-wide)
        public long? StoreId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Store Store { get; set; }
        public ICollection<VoucherUsage> Usages { get; set; }
    }

    public class VoucherUsage
    {
        public Guid Id { get; set; }
        
        public Guid VoucherId { get; set; }
        
        public long UserId { get; set; }
        
        public Guid OrderId { get; set; }
        
        // Actual discount amount applied
        [Column(TypeName = "money")]
        public decimal DiscountAmount { get; set; }
        
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
        
        public Voucher Voucher { get; set; }
        public User User { get; set; }
        public Order Order { get; set; }
    }
}
