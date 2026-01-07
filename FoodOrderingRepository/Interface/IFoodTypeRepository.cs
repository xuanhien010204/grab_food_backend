using FoodOrderingCore.Dto;

namespace FoodOrderingRepository.Interface
{
    public interface IFoodTypeRepository
    {
        Task<IEnumerable<FoodTypeDto>> GetAllFoodTypeAsync();
    }
}
