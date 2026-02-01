using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IDeliveryAddressRepository
    {
        Task<IEnumerable<DeliveryAddressDto>> GetUserAddressesAsync(long userId);
        Task<DeliveryAddressDto> GetAddressByIdAsync(long addressId, long userId);
        Task<DeliveryAddressDto> CreateAddressAsync(CreateDeliveryAddressRequest request, long userId);
        Task<DeliveryAddressDto> UpdateAddressAsync(long addressId, UpdateDeliveryAddressRequest request, long userId);
        Task<bool> DeleteAddressAsync(long addressId, long userId);
        Task<DeliveryAddressDto> SetDefaultAddressAsync(long addressId, long userId);
        Task<DeliveryAddressDto> GetDefaultAddressAsync(long userId);
    }
}
