
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IStoreRepository
    {
        Task<IEnumerable<StoreDto>> GetAllFoodStore();
        Task<IEnumerable<StoreDto>> GetAllFoodStoreByTenant(int tenantId);
        Task<StoreDto> GetStoreDetail(long id);
    }
}
