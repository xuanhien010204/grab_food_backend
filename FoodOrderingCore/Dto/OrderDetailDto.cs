namespace FoodOrderingCore.Dto
{
    public class OrderDetailDto
    {
        public Guid OrderId { get; set; }
        public Guid FoodStoreId { get; set; }

        // Food info
        public long FoodId { get; set; }
        public string FoodName { get; set; }
        public string FoodImage { get; set; }

        // Size info (if applicable)
        public int? SizeId { get; set; }
        public string SizeName { get; set; }

        // Price & Quantity
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
