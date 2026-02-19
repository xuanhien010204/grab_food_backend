
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IStoreRepository
    {
        Task<IEnumerable<StoreDto>> GetAllFoodStore();
        Task<IEnumerable<StoreDto>> GetAllFoodStoreByTenant(int tenantId);
        Task<StoreDto> GetStoreDetail(long id);
        Task<StoreDto> GetStoreByManagerId(long managerId);
        Task<StoreDto> CreateStoreAsync(RegisterManagerRequest request, long managerId);
        Task<bool> ApproveStoreAsync(long storeId);
        Task<IEnumerable<StoreDto>> GetPendingStores();
    }
}
