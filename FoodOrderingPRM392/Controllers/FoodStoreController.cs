using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/food-stores")]
    [ApiController]
    public class FoodStoreController : ControllerBase
    {
        private IFoodStoreRepository foodStoreRepository;

        public FoodStoreController(IFoodStoreRepository foodStoreRepository)
        {
            this.foodStoreRepository = foodStoreRepository;
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
    }
}
