using Azure;
using FoodOrderingCore.Data;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/food-types")]
    [ApiController]
    public class FoodTypeController : ControllerBase
    {
        private readonly IFoodTypeRepository foodTypeRepo;

        public FoodTypeController(IFoodTypeRepository foodTypeRepo)
        {
            this.foodTypeRepo = foodTypeRepo;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodType(int id) 
        {
            var respone = await foodTypeRepo.GetFoodTypeByIdAsync(id);
            if (respone == null)
                return NotFound(new ParentResponse { Message = "FoodType not found" });
            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = respone
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFoodTypes()
        {
            var list = await foodTypeRepo.GetAllFoodTypeAsync();

            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = list
            });
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateFoodType([FromBody] FoodTypeCreateRequest request)
        {
            var createdFoodType = await foodTypeRepo.CreateFoodTypeAsync(request);
            var respone = await foodTypeRepo.GetFoodTypeByIdAsync(createdFoodType);
            return Ok(new ParentResultResponse
            {
                Message = "Food type created successfully",
                Result = respone
            });
        }
        [HttpPut]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateFoodType([FromBody] FoodTypeUpdateRequest request)
        {
            var updatedFoodType = await foodTypeRepo.UpdateFoodTypeAsync(request);
            var respone = await foodTypeRepo.GetFoodTypeByIdAsync(updatedFoodType);
            return Ok(new ParentResultResponse
            {
                Message = "Food type updated successfully",
                Result = respone
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteFoodType(int id)
        {
            var respone = await foodTypeRepo.DeleteFoodTypeAsync(id);
            if (respone == false)
                return NotFound(new ParentResponse { Message = "FoodType not found" });
            return Ok(new ParentResponse
            {
                Message = "Food type deleted successfully"
            });
        }
    }
}
