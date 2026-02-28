using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Constants;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoodOrderingRepository.Implement
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;

        public ReviewRepository(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewRequest request, long userId)
        {
            // Validate order
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);

            if (order == null)
                throw new BadRequestException(ReviewMessages.NotYourOrder);

            if (order.Status != OrderStatus.Completed)
                throw new BadRequestException(ReviewMessages.OrderNotCompleted);

            // Check if already reviewed
            var existingReview = await _context.Reviews
                .AnyAsync(r => r.OrderId == request.OrderId);

            if (existingReview)
                throw new BadRequestException(ReviewMessages.AlreadyReviewed);

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderId = request.OrderId,
                StoreId = request.StoreId ?? order.StoreId,
                FoodId = request.FoodId,
                Rating = request.Rating,
                Comment = request.Comment,
                Images = request.Images != null ? JsonSerializer.Serialize(request.Images) : null,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update store rating (must be after SaveChanges so Dapper queries can see the new review)
            if (review.StoreId.HasValue)
            {
                await UpdateStoreRatingAsync(review.StoreId.Value);
            }

            // Update food rating
            if (review.FoodId.HasValue)
            {
                await UpdateFoodRatingAsync(review.FoodId.Value);
            }

            return await GetReviewByIdAsync(review.Id);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(Guid reviewId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT r.Id, r.UserId, u.Name as UserName, u.AvatarUrl as UserAvatar,
                       r.OrderId, r.StoreId, s.Name as StoreName,
                       r.FoodId, f.Name as FoodName,
                       r.Rating, r.Comment, r.Images,
                       r.StoreReply, r.StoreReplyAt, r.CreatedAt
                FROM Reviews r
                JOIN Users u ON r.UserId = u.Id
                LEFT JOIN Stores s ON r.StoreId = s.Id
                LEFT JOIN Foods f ON r.FoodId = f.Id
                WHERE r.Id = @reviewId AND r.IsVisible = 1";

            var review = await con.QueryFirstOrDefaultAsync<ReviewDto>(sql, new { reviewId });

            if (review != null && !string.IsNullOrEmpty(review.Images?.ToString()))
            {
                // Images is stored as JSON, parse it
                try
                {
                    var imagesJson = review.GetType().GetProperty("Images")?.GetValue(review)?.ToString();
                    if (!string.IsNullOrEmpty(imagesJson))
                    {
                        review.Images = JsonSerializer.Deserialize<List<string>>(imagesJson);
                    }
                }
                catch { }
            }

            return review;
        }

        public async Task<IEnumerable<ReviewDto>> GetStoreReviewsAsync(long storeId, int pageNumber = 1, int pageSize = 20)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT r.Id, r.UserId, u.Name as UserName, u.AvatarUrl as UserAvatar,
                       r.OrderId, r.StoreId, r.FoodId,
                       r.Rating, r.Comment, r.Images,
                       r.StoreReply, r.StoreReplyAt, r.CreatedAt
                FROM Reviews r
                JOIN Users u ON r.UserId = u.Id
                WHERE r.StoreId = @storeId AND r.IsVisible = 1
                ORDER BY r.CreatedAt DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            return await con.QueryAsync<ReviewDto>(sql, new { storeId, offset, pageSize });
        }

        public async Task<IEnumerable<ReviewDto>> GetFoodReviewsAsync(long foodId, int pageNumber = 1, int pageSize = 20)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT r.Id, r.UserId, u.Name as UserName, u.AvatarUrl as UserAvatar,
                       r.OrderId, r.StoreId, r.FoodId,
                       r.Rating, r.Comment, r.Images,
                       r.StoreReply, r.StoreReplyAt, r.CreatedAt
                FROM Reviews r
                JOIN Users u ON r.UserId = u.Id
                WHERE r.FoodId = @foodId AND r.IsVisible = 1
                ORDER BY r.CreatedAt DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            return await con.QueryAsync<ReviewDto>(sql, new { foodId, offset, pageSize });
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(long userId, int pageNumber = 1, int pageSize = 20)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT r.Id, r.UserId, u.Name as UserName, u.AvatarUrl as UserAvatar,
                       r.OrderId, r.StoreId, s.Name as StoreName,
                       r.FoodId, f.Name as FoodName,
                       r.Rating, r.Comment, r.Images,
                       r.StoreReply, r.StoreReplyAt, r.CreatedAt
                FROM Reviews r
                JOIN Users u ON r.UserId = u.Id
                LEFT JOIN Stores s ON r.StoreId = s.Id
                LEFT JOIN Foods f ON r.FoodId = f.Id
                WHERE r.UserId = @userId AND r.IsVisible = 1
                ORDER BY r.CreatedAt DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            return await con.QueryAsync<ReviewDto>(sql, new { userId, offset, pageSize });
        }

        public async Task<ReviewDto> StoreReplyAsync(Guid reviewId, StoreReplyRequest request)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                throw new BadRequestException(ReviewMessages.NotFound);

            review.StoreReply = request.Reply;
            review.StoreReplyAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetReviewByIdAsync(reviewId);
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId, long userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                return false;

            review.IsVisible = false;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Recalculate ratings (must be after SaveChanges so Dapper queries reflect the hidden review)
            if (review.StoreId.HasValue)
                await UpdateStoreRatingAsync(review.StoreId.Value);

            if (review.FoodId.HasValue)
                await UpdateFoodRatingAsync(review.FoodId.Value);
            return true;
        }

        public async Task<bool> CanUserReviewOrderAsync(Guid orderId, long userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Status != OrderStatus.Completed)
                return false;

            var hasReviewed = await _context.Reviews.AnyAsync(r => r.OrderId == orderId);
            return !hasReviewed;
        }

        public async Task<ReviewStatsDto> GetStoreReviewStatsAsync(long storeId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT 
                    ISNULL(AVG(CAST(Rating AS DECIMAL(3,2))), 0) as AverageRating,
                    COUNT(*) as TotalReviews,
                    SUM(CASE WHEN Rating = 5 THEN 1 ELSE 0 END) as FiveStarCount,
                    SUM(CASE WHEN Rating = 4 THEN 1 ELSE 0 END) as FourStarCount,
                    SUM(CASE WHEN Rating = 3 THEN 1 ELSE 0 END) as ThreeStarCount,
                    SUM(CASE WHEN Rating = 2 THEN 1 ELSE 0 END) as TwoStarCount,
                    SUM(CASE WHEN Rating = 1 THEN 1 ELSE 0 END) as OneStarCount
                FROM Reviews
                WHERE StoreId = @storeId AND IsVisible = 1";

            return await con.QueryFirstOrDefaultAsync<ReviewStatsDto>(sql, new { storeId })
                ?? new ReviewStatsDto();
        }

        private async Task UpdateStoreRatingAsync(long storeId)
        {
            var stats = await GetStoreReviewStatsAsync(storeId);
            
            await _context.Stores
                .Where(s => s.Id == storeId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Rating, stats.AverageRating)
                    .SetProperty(x => x.ReviewCount, stats.TotalReviews));
        }

        private async Task UpdateFoodRatingAsync(long foodId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                UPDATE Foods SET 
                    Rating = ISNULL((SELECT AVG(CAST(Rating AS DECIMAL(3,2))) FROM Reviews WHERE FoodId = @foodId AND IsVisible = 1), 0),
                    ReviewCount = (SELECT COUNT(*) FROM Reviews WHERE FoodId = @foodId AND IsVisible = 1)
                WHERE Id = @foodId";

            await con.ExecuteAsync(sql, new { foodId });
        }
    }
}
