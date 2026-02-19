using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IFoodStoreRepository
    {
        Task<IEnumerable<FoodStoreDto>> GetAllFoodStore(FoodStoreFilterRequest request);
        Task<IEnumerable<FoodStoreDto>> GetFoodStoresByStoreId(long storeId);
        Task<FoodStoreDto> CreateFoodStoreAsync(FoodStoreDto request);
        Task<bool> UpdateFoodStoreAsync(FoodStoreUpdateRequest request);
        Task<bool> DeleteFoodStoreAsync(Guid id);
    }
}
