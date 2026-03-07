using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class SendMessageRequest
    {
        [Required]
        public long ReceiverId { get; set; }

        [Required]
        public long StoreId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }
    }
}
