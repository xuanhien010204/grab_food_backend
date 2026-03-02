using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/food-stores")]
    [ApiController]
    public class FoodStoreController : ControllerBase
    {
        private IFoodStoreRepository foodStoreRepository;
        private IStoreRepository storeRepository;

        public FoodStoreController(IFoodStoreRepository foodStoreRepository, IStoreRepository storeRepository)
        {
            this.foodStoreRepository = foodStoreRepository;
            this.storeRepository = storeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFoodStores([FromQuery] FoodStoreFilterRequest request)
        {
            var list = await foodStoreRepository.GetAllFoodStore(request);

            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = list
            });
        }

        /// <summary>
        /// Get all food stores (menu items) for a specific store
        /// </summary>
        [HttpGet("store/{storeId}")]
        public async Task<IActionResult> GetFoodStoresByStoreId(long storeId)
        {
            var list = await foodStoreRepository.GetFoodStoresByStoreId(storeId);

            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = list
            });
        }

        /// <summary>
        /// Manager see list foodstores
        /// </summary>
        [HttpGet("my-store")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMyStoreFoodStores()
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var store = await storeRepository.GetStoreByManagerId(userId);

            if (store == null)
                return BadRequest(new ParentResponse { Message = "You don't have any store" });

            var list = await foodStoreRepository.GetFoodStoresByStoreId(store.Id);

            return Ok(new ParentResultResponse { Message = "Success", Result = list });
        }

        /// <summary>
        /// Manager creare in my store
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateFoodStore([FromBody] FoodStoreDto request)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var store = await storeRepository.GetStoreByManagerId(userId);

            if (store == null)
                return BadRequest(new ParentResponse { Message = "You don't have any store" });

            if (store.Id != request.StoreId)
                return Forbid();

            var result = await foodStoreRepository.CreateFoodStoreAsync(request);

            return Ok(new ParentResultResponse { Message = "Create foodstore successfully", Result = result });
        }

        /// <summary>
        /// Manager update foodstore in my store
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateFoodStore([FromBody] FoodStoreUpdateRequest request)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var store = await storeRepository.GetStoreByManagerId(userId);

            if (store == null)
                return BadRequest(new ParentResponse { Message = "You don't have any store" });

            var success = await foodStoreRepository.UpdateFoodStoreAsync(request);

            return success
                ? Ok(new ParentResponse { Message = "Update foodstore successfully" })
                : BadRequest(new ParentResponse { Message = "Update foodstore failed" });
        }

        /// <summary>
        /// Manager delete foodstore in my store
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteFoodStore(Guid id)
        {
            long userId = Convert.ToInt64(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var store = await storeRepository.GetStoreByManagerId(userId);

            if (store == null)
                return BadRequest(new ParentResponse { Message = "Bạn chưa có store nào" });

            var success = await foodStoreRepository.DeleteFoodStoreAsync(id);

            return success
                ? Ok(new ParentResponse { Message = "Delete foodstore successfully" })
                : BadRequest(new ParentResponse { Message = "Delete foodstore failed" });
        }
    }
}
