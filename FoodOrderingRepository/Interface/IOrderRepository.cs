using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;

namespace FoodOrderingRepository.Interface
{
    public interface IOrderRepository
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, long userId);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);
        Task<OrderDto> GetOrderDetailAsync(Guid orderId);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(long userId, OrderStatus? status = null);

        // Get all orders for a store (for store manager)
        Task<IEnumerable<OrderDto>> GetStoreOrdersAsync(long storeId, OrderStatus? status = null);

        // Update order status
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string reason = null);

        // Cancel order
        Task<OrderDto> CancelOrderAsync(Guid orderId, string reason, long userId);

        // Process payment for order (deduct from wallet)
        Task<bool> ProcessPaymentAsync(Guid orderId);

        // Legacy methods (for backward compatibility)
        Task<IEnumerable<OrderDto>> GetAllOrderAsync(long userId);
        Task<int> CreateOrderAsync(Dictionary<string, int> orderDetail, long userId);
        Task<DetailOrderResponse> GetDetailOrder(Guid orderId);
    }
}
