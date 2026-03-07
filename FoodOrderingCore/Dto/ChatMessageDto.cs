namespace FoodOrderingCore.Dto
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public long ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class ChatConversationDto
    {
        public long OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserAvatar { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
