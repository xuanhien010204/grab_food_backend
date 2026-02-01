using FoodOrderingCore.Constants;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteController(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetFavoriteStores()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var favorites = await _favoriteRepository.GetFavoriteStoresAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = FavoriteMessages.GetSuccess,
                Result = favorites
            });
        }

        [HttpGet("foods")]
        public async Task<IActionResult> GetFavoriteFoods()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var favorites = await _favoriteRepository.GetFavoriteFoodsAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = FavoriteMessages.GetSuccess,
                Result = favorites
            });
        }

        [HttpPost("stores/{storeId:long}")]
        public async Task<IActionResult> AddStoreFavorite(long storeId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var favorite = await _favoriteRepository.AddStoreFavoriteAsync(storeId, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = FavoriteMessages.AddSuccess,
                Result = favorite
            });
        }

        [HttpPost("foods/{foodId:long}")]
        public async Task<IActionResult> AddFoodFavorite(long foodId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var favorite = await _favoriteRepository.AddFoodFavoriteAsync(foodId, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = FavoriteMessages.AddSuccess,
                Result = favorite
            });
        }

        [HttpDelete("stores/{storeId:long}")]
        public async Task<IActionResult> RemoveStoreFavorite(long storeId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _favoriteRepository.RemoveStoreFavoriteAsync(storeId, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = FavoriteMessages.NotFound });

            return Ok(new ParentResponse { Message = FavoriteMessages.RemoveSuccess });
        }

        [HttpDelete("foods/{foodId:long}")]
        public async Task<IActionResult> RemoveFoodFavorite(long foodId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _favoriteRepository.RemoveFoodFavoriteAsync(foodId, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = FavoriteMessages.NotFound });

            return Ok(new ParentResponse { Message = FavoriteMessages.RemoveSuccess });
        }

        [HttpGet("stores/{storeId:long}/check")]
        public async Task<IActionResult> IsStoreFavorited(long storeId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isFavorited = await _favoriteRepository.IsStoreFavoritedAsync(storeId, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = ResponseMessages.Success,
                Result = new { IsFavorited = isFavorited }
            });
        }

        [HttpGet("foods/{foodId:long}/check")]
        public async Task<IActionResult> IsFoodFavorited(long foodId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isFavorited = await _favoriteRepository.IsFoodFavoritedAsync(foodId, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = ResponseMessages.Success,
                Result = new { IsFavorited = isFavorited }
            });
        }
    }
}
