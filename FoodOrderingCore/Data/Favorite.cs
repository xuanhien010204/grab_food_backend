namespace FoodOrderingCore.Data
{
    public class Favorite
    {
        public long Id { get; set; }
        
        public long UserId { get; set; }
        
        public long? StoreId { get; set; }
        
        public long? FoodId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public User User { get; set; }
        public Store Store { get; set; }
        public Food Food { get; set; }
    }
}
