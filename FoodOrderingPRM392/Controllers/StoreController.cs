using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/stores")]
    [ApiController]
    //[Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IStoreRepository _storeRepository;

        public StoreController(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStoreAsync([FromQuery] StoreFilterRequest request) {
            var list = await _storeRepository.GetAllFoodStore(request);
            return Ok(new ParentResultResponse { Message = "Success", Result = list});
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetStoreDetailAsync(long id)
        {
            var store = await _storeRepository.GetStoreDetail(id);

            if (store == null) throw new BadRequestException();

            return Ok(new ParentResultResponse { Message = "Success", Result = store });
        }
    }
}
