using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Data
{
    public class Notification
    {
        public Guid Id { get; set; }
        
        public long UserId { get; set; }
        
        [Column(TypeName = "nvarchar(200)")]
        public string Title { get; set; }
        
        [Column(TypeName = "nvarchar(1000)")]
        public string Content { get; set; }
        
        public NotificationType Type { get; set; }
        
        // Reference ID (OrderId, VoucherId, etc.)
        [Column(TypeName = "varchar(100)")]
        public string ReferenceId { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string ImageUrl { get; set; }
        
        // Deep link for navigation
        [Column(TypeName = "varchar(200)")]
        public string DeepLink { get; set; }
        
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public User User { get; set; }
    }
}
