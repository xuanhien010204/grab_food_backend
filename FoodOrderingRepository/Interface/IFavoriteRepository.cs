using FoodOrderingCore.Dto;

namespace FoodOrderingRepository.Interface
{
    public interface IFavoriteRepository
    {
        // Add store to favorites
        Task<FavoriteDto> AddStoreFavoriteAsync(long storeId, long userId);
        
        // Add food to favorites
        Task<FavoriteDto> AddFoodFavoriteAsync(long foodId, long userId);
        
        // Remove from favorites
        Task<bool> RemoveFavoriteAsync(long favoriteId, long userId);
        
        // Remove store from favorites
        Task<bool> RemoveStoreFavoriteAsync(long storeId, long userId);
        
        // Remove food from favorites
        Task<bool> RemoveFoodFavoriteAsync(long foodId, long userId);
        
        // Get user's favorite stores
        Task<IEnumerable<FavoriteDto>> GetFavoriteStoresAsync(long userId);
        
        // Get user's favorite foods
        Task<IEnumerable<FavoriteDto>> GetFavoriteFoodsAsync(long userId);
        // Check if store is favorited
        Task<bool> IsStoreFavoritedAsync(long storeId, long userId);
        // Check if food is favorited
        Task<bool> IsFoodFavoritedAsync(long foodId, long userId);
    }
}
