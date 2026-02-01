using FoodOrderingCore.Constants;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var review = await _reviewRepository.CreateReviewAsync(request, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.CreateSuccess,
                Result = review
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReview(Guid id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);

            if (review == null)
                return NotFound(new ParentResponse { Message = ReviewMessages.NotFound });

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.GetSuccess,
                Result = review
            });
        }

        /// <summary>
        /// Get reviews for a store
        /// </summary>
        [HttpGet("store/{storeId:long}")]
        public async Task<IActionResult> GetStoreReviews(
            long storeId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var reviews = await _reviewRepository.GetStoreReviewsAsync(storeId, pageNumber, pageSize);
            var stats = await _reviewRepository.GetStoreReviewStatsAsync(storeId);

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.GetSuccess,
                Result = new
                {
                    Stats = stats,
                    Reviews = reviews,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            });
        }

        // Get reviews for a food item
        [HttpGet("food/{foodId:long}")]
        public async Task<IActionResult> GetFoodReviews(
            long foodId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var reviews = await _reviewRepository.GetFoodReviewsAsync(foodId, pageNumber, pageSize);

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.GetSuccess,
                Result = new
                {
                    Reviews = reviews,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            });
        }

        // Get current user's reviews
        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var reviews = await _reviewRepository.GetUserReviewsAsync(userId.Value, pageNumber, pageSize);

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.GetSuccess,
                Result = new
                {
                    Reviews = reviews,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            });
        }

        // Store reply to a review
        [HttpPost("{id:guid}/reply")]
        [Authorize]
        public async Task<IActionResult> StoreReply(Guid id, [FromBody] StoreReplyRequest request)
        {
            var review = await _reviewRepository.StoreReplyAsync(id, request);

            return Ok(new ParentResultResponse
            {
                Message = ReviewMessages.ReplySuccess,
                Result = review
            });
        }

        // Delete a review (only by owner)
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _reviewRepository.DeleteReviewAsync(id, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = ReviewMessages.NotFound });

            return Ok(new ParentResponse { Message = ReviewMessages.DeleteSuccess });
        }

        // Check if user can review an order
        [HttpGet("can-review/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> CanReviewOrder(Guid orderId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var canReview = await _reviewRepository.CanUserReviewOrderAsync(orderId, userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = ResponseMessages.Success,
                Result = new { CanReview = canReview }
            });
        }
    }
}
