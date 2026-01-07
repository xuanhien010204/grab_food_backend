using FoodOrderingCore.Dto;

namespace FoodOrderingCore.Response
{
    public class DetailOrderResponse
    {
        public OrderDto Order { set; get; }
        public StoreDto Store { set; get; }
        public IEnumerable<OrderDetailDto> OrderDetails { set; get; }
    }
}
