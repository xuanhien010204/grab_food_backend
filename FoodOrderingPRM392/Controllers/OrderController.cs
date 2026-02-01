using FoodOrderingCore.Constants;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            _logger.LogInformation("Creating order: UserId={UserId}, StoreId={StoreId}, Items={Items}",
                userId, request.StoreId, request.Items.Count);

            var order = await _orderRepository.CreateOrderAsync(request, userId.Value);

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.CreateSuccess, 
                Result = order 
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderDetail(Guid id)
        {
            var order = await _orderRepository.GetOrderDetailAsync(id);

            if (order == null)
                return NotFound(new ParentResponse { Message = OrderMessages.OrderNotFound });

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.GetOrderSuccess, 
                Result = order 
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory([FromQuery] OrderStatus? status = null)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var orders = await _orderRepository.GetUserOrdersAsync(userId.Value, status);

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.GetOrdersSuccess, 
                Result = orders 
            });
        }

        [HttpGet("store/{storeId:long}")]
        public async Task<IActionResult> GetStoreOrders(long storeId, [FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderRepository.GetStoreOrdersAsync(storeId, status);

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.GetOrdersSuccess, 
                Result = orders 
            });
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            _logger.LogInformation("Updating order status: OrderId={OrderId}, NewStatus={Status}",
                id, request.Status);

            var order = await _orderRepository.UpdateOrderStatusAsync(id, request.Status, request.Reason);

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.UpdateStatusSuccess, 
                Result = order 
            });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            _logger.LogInformation("Cancelling order: OrderId={OrderId}, UserId={UserId}",
                id, userId);

            var order = await _orderRepository.CancelOrderAsync(id, request.Reason, userId.Value);

            return Ok(new ParentResultResponse 
            { 
                Message = OrderMessages.CancelSuccess, 
                Result = order 
            });
        }

        // Legacy: Create order with dictionary (backward compatibility)
        [HttpPost("legacy")]
        public async Task<IActionResult> CreateOrderLegacy([FromBody] IDictionary<string, int> orderDetail)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            int count = await _orderRepository.CreateOrderAsync(
                new Dictionary<string, int>(orderDetail), userId.Value);

            return Ok(new ParentResponse { Message = ResponseMessages.Success });
        }

        // Legacy: Get order detail (backward compatibility)
        [HttpGet("{id:guid}/legacy")]
        public async Task<IActionResult> GetDetailOrderLegacy(Guid id)
        {
            var response = await _orderRepository.GetDetailOrder(id);
            return Ok(new ParentResultResponse { Message = ResponseMessages.Success, Result = response });
        }
    }
    public class CancelOrderRequest
    {
        public string Reason { get; set; }
    }
}
