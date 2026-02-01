using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;

        public FavoriteRepository(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public async Task<FavoriteDto> AddStoreFavoriteAsync(long storeId, long userId)
        {
            // Check if already favorited
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.StoreId == storeId);

            if (existing != null)
            {
                return await GetFavoriteDtoAsync(existing.Id);
            }

            var favorite = new Favorite
            {
                UserId = userId,
                StoreId = storeId,
                FoodId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return await GetFavoriteDtoAsync(favorite.Id);
        }

        public async Task<FavoriteDto> AddFoodFavoriteAsync(long foodId, long userId)
        {
            // Check if already favorited
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FoodId == foodId);

            if (existing != null)
            {
                return await GetFavoriteDtoAsync(existing.Id);
            }

            var favorite = new Favorite
            {
                UserId = userId,
                StoreId = null,
                FoodId = foodId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return await GetFavoriteDtoAsync(favorite.Id);
        }

        public async Task<bool> RemoveFavoriteAsync(long favoriteId, long userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStoreFavoriteAsync(long storeId, long userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.StoreId == storeId);

            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFoodFavoriteAsync(long foodId, long userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FoodId == foodId);

            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FavoriteDto>> GetFavoriteStoresAsync(long userId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT f.Id, f.UserId, f.StoreId, f.CreatedAt,
                       s.Name as StoreName, s.ImageSrc as StoreImage, 
                       s.Address as StoreAddress, s.Rating as StoreRating
                FROM Favorites f
                JOIN Stores s ON f.StoreId = s.Id
                WHERE f.UserId = @userId AND f.StoreId IS NOT NULL
                ORDER BY f.CreatedAt DESC";

            return await con.QueryAsync<FavoriteDto>(sql, new { userId });
        }

        public async Task<IEnumerable<FavoriteDto>> GetFavoriteFoodsAsync(long userId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT f.Id, f.UserId, f.FoodId, f.CreatedAt,
                       fd.Name as FoodName, fd.ImageSrc as FoodImage,
                       (SELECT TOP 1 Price FROM FoodStores WHERE FoodId = fd.Id) as FoodPrice
                FROM Favorites f
                JOIN Foods fd ON f.FoodId = fd.Id
                WHERE f.UserId = @userId AND f.FoodId IS NOT NULL
                ORDER BY f.CreatedAt DESC";

            return await con.QueryAsync<FavoriteDto>(sql, new { userId });
        }

        public async Task<bool> IsStoreFavoritedAsync(long storeId, long userId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.StoreId == storeId);
        }

        public async Task<bool> IsFoodFavoritedAsync(long foodId, long userId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.FoodId == foodId);
        }

        private async Task<FavoriteDto> GetFavoriteDtoAsync(long favoriteId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT f.Id, f.UserId, f.StoreId, f.FoodId, f.CreatedAt,
                       s.Name as StoreName, s.ImageSrc as StoreImage, 
                       s.Address as StoreAddress, s.Rating as StoreRating,
                       fd.Name as FoodName, fd.ImageSrc as FoodImage
                FROM Favorites f
                LEFT JOIN Stores s ON f.StoreId = s.Id
                LEFT JOIN Foods fd ON f.FoodId = fd.Id
                WHERE f.Id = @favoriteId";

            return await con.QueryFirstOrDefaultAsync<FavoriteDto>(sql, new { favoriteId });
        }
    }
}
