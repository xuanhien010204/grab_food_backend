using Azure;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Implement;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/foods")]
    [Authorize(Roles = "Admin, Manager")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly IFoodRepository _foodRepository;
        public FoodController(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFood()
        {
            var list = await _foodRepository.GetAllFoodAsync();

            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = list
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodById(long id)
        {
            var respone = await _foodRepository.GetFoodByIdAsync(id);

            if (respone == null)
                return NotFound(new ParentResponse { Message = "FoodType not found" });
            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = respone
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateFood([FromBody] FoodRequest request)
        {
            var respone = await _foodRepository.CreateFoodAsync(request);
            return Ok(new ParentResultResponse
            {
                Message = "Food type created successfully",
                Result = respone
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateFoodType([FromBody] FoodUpdate request)
        {
            var updatedFoodType = await _foodRepository.UpdateFoodAsync(request);
            var respone = await _foodRepository.GetFoodByIdAsync(updatedFoodType);
            return Ok(new ParentResultResponse
            {
                Message = "Food type updated successfully",
                Result = respone
            });
        }
    }
}
