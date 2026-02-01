using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Review
    {
        public Guid Id { get; set; }
        
        public long UserId { get; set; }
        public Guid OrderId { get; set; }
        public long? StoreId { get; set; }
        public long? FoodId { get; set; }
        
        // Rating 1-5 stars
        public int Rating { get; set; }
        
        [Column(TypeName = "nvarchar(1000)")]
        public string Comment { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string Images { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string StoreReply { get; set; }
        public DateTime? StoreReplyAt { get; set; }
        public bool IsVisible { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; }
        public Order Order { get; set; }
        public Store Store { get; set; }
        public Food Food { get; set; }
    }
}
