using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class FoodStoreUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }

        public decimal? Price { get; set; }

        public int? SizeId { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
