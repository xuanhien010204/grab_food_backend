using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Dto
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public string TypeName => Type.ToString();
        public string ReferenceId { get; set; }
        public string ImageUrl { get; set; }
        public string DeepLink { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Relative time (e.g., "5 minute before", "2 hour before")
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.UtcNow - CreatedAt;
                if (diff.TotalMinutes < 1) return "Vừa xong";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
                return CreatedAt.ToString("dd/MM/yyyy");
            }
        }
    }
}
