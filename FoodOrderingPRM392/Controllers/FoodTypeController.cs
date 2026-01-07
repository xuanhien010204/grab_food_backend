using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
