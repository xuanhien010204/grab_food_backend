using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IUserRepository
    {
        Task<UserDto> LoginAsync(LoginRequest request);

        Task<int> RegisterAsync(RegisterRequest request);
        Task RecharseWalletAmountAsync(decimal money, long userId);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetById(long id);
        Task CreateAsync<E>(E request);
        Task UpdateAysnc<E>(E request);
        Task Delete<E>(E id);
        Task UpdateTempCartMetaAsync(Cart cart, long userId);
        Task DeleteTempCartMetaAsync(long userId);
    }
}
