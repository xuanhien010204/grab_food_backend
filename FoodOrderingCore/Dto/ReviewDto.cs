namespace FoodOrderingCore.Dto
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public Guid OrderId { get; set; }
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        public long? FoodId { get; set; }
        public string FoodName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<string> Images { get; set; }
        public string StoreReply { get; set; }
        public DateTime? StoreReplyAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
