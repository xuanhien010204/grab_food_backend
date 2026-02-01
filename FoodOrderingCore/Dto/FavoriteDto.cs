namespace FoodOrderingCore.Dto
{
    public class FavoriteDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        
        // Store info (if store favorite)
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreImage { get; set; }
        public string StoreAddress { get; set; }
        public decimal StoreRating { get; set; }
        
        // Food info (if food favorite)
        public long? FoodId { get; set; }
        public string FoodName { get; set; }
        public string FoodImage { get; set; }
        public decimal? FoodPrice { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
