
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IStoreRepository
    {
        Task<IEnumerable<StoreDto>> GetAllFoodStore(StoreFilterRequest request);
        Task<StoreDto> GetStoreDetail(long id);
    }
}
