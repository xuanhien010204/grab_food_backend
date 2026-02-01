using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Dto
{
    public class VoucherDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public VoucherType Type { get; set; }
        public string TypeName => Type.ToString();
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        
        // Is voucher currently valid?
        public bool IsValid => IsActive && 
            DateTime.UtcNow >= StartDate && 
            DateTime.UtcNow <= EndDate &&
            (UsageLimit == null || UsedCount < UsageLimit);
        
        /// Formatted discount text (e.g., "Giảm 10%" or "Giảm 20,000đ")
        public string DiscountText => Type switch
        {
            VoucherType.Percent => $"Giảm {Value}%",
            VoucherType.FixedAmount => $"Giảm {Value:N0}đ",
            VoucherType.FreeShipping => "Miễn phí vận chuyển",
            _ => ""
        };
    }
}
