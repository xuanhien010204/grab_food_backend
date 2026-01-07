namespace FoodOrderingCore.Dto
{
    public class OrderDetailDto
    {
        public Guid OrderId { set; get; }
        public OrderDto Order { set; get; }
        public Guid FoodStoreId { set; get; }
        public FoodDto Food { set; get; }
        public decimal Price { set; get; }
        public int Quantity { set; get; }
        public decimal Total { set; get; }
    }
}
