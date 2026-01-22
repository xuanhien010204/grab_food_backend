using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IFoodStoreRepository
    {
        Task<IEnumerable<FoodStoreDto>> GetAllFoodStore(FoodStoreFilterRequest request);
        Task<FoodStoreDto> CreateFoodStoreAsync(FoodStoreDto request);
    }
}
