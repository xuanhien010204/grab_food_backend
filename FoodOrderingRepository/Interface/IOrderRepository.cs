using FoodOrderingCore.Dto;
using FoodOrderingCore.Response;

namespace FoodOrderingRepository.Interface
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderDto>> GetAllOrderAsync(long userId);
        Task<int> CreateOrderAsync(Dictionary<string, int> orderDetail, long userId);
        Task<DetailOrderResponse> GetDetailOrder(Guid orderId);
    }
}
