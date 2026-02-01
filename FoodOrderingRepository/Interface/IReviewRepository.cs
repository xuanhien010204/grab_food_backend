using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IReviewRepository
    {
        Task<ReviewDto> CreateReviewAsync(CreateReviewRequest request, long userId);
        Task<ReviewDto> GetReviewByIdAsync(Guid reviewId);
        Task<IEnumerable<ReviewDto>> GetStoreReviewsAsync(long storeId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<ReviewDto>> GetFoodReviewsAsync(long foodId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(long userId, int pageNumber = 1, int pageSize = 20);
        Task<ReviewDto> StoreReplyAsync(Guid reviewId, StoreReplyRequest request);
        Task<bool> DeleteReviewAsync(Guid reviewId, long userId);
        Task<bool> CanUserReviewOrderAsync(Guid orderId, long userId);
        
        // Get review statistics for a store
        Task<ReviewStatsDto> GetStoreReviewStatsAsync(long storeId);
    }

    public class ReviewStatsDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }
}
