using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IFoodTypeRepository
    {
        Task<IEnumerable<FoodTypeDto>> GetAllFoodTypeAsync();
        Task<FoodTypeDto> GetFoodTypeByIdAsync(int id);
        Task<int> CreateFoodTypeAsync(FoodTypeCreateRequest foodTypeDto);
        Task<int> UpdateFoodTypeAsync(FoodTypeUpdateRequest foodTypeDto);
        Task<bool> DeleteFoodTypeAsync(int id);
    }
}
