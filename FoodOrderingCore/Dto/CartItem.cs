namespace FoodOrderingCore.Dto
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public FoodStoreDto FoodStore { set; get; }
    }
}
