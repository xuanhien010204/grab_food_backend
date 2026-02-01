using System.ComponentModel.DataAnnotations;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Request
{
    public class CreateVoucherRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public VoucherType Type { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; } = 0;

        public decimal? MaxDiscount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? UsageLimit { get; set; }
        public int? UsageLimitPerUser { get; set; } = 1;

        /// <summary>
        /// Store ID if store-specific (null = platform-wide)
        /// </summary>
        public long? StoreId { get; set; }
    }

    public class UpdateVoucherRequest
    {
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ApplyVoucherRequest
    {
        [Required]
        public string Code { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal OrderAmount { get; set; }

        public long? StoreId { get; set; }
    }
}
