namespace FoodOrderingCore.Request
{
    public class OrderCreateRequest
    {
        public Guid FoodStoreId { set; get; }
        public int Quantity { set; get; }
    }
}
