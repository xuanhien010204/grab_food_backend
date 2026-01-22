using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IFoodRepository
    {
        Task<IEnumerable<FoodDto>> GetAllFoodAsync();
        Task<FoodDto> GetFoodByIdAsync(long id);
        Task<FoodDto> CreateFoodAsync(FoodRequest request);
        Task<int> UpdateFoodAsync(FoodUpdate update);
    }
}
