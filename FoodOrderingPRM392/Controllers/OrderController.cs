using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetDetailOrder(Guid id)
        {
            var response = await orderRepository.GetDetailOrder(id);

            return Ok(new ParentResultResponse { Message = "Success", Result = response });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPurchaseHistoryAsync()
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier).Value);

            var list = await orderRepository.GetAllOrderAsync(userId);

            return Ok(new ParentResultResponse { Message = "Success", Result = list });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody] IDictionary<string, int> orderDetail)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier).Value);
            int count = await orderRepository.CreateOrderAsync(new Dictionary<string, int>(orderDetail), userId);

            return Ok(new ParentResponse { Message = "Success"});
        }
    }
}
