using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid OrderId { get; set; }
        public long? StoreId { get; set; }
        public long? FoodId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
        public List<string> Images { get; set; }
    }

    public class StoreReplyRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reply { get; set; }
    }
}
