using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }

        public long SenderId { get; set; }

        public long ReceiverId { get; set; }

        // Store context for the conversation (user <-> store manager)
        public long StoreId { get; set; }

        [Column(TypeName = "nvarchar(2000)")]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public Store Store { get; set; }
    }
}
