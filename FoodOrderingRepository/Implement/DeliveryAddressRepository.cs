using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class DeliveryAddressRepository : IDeliveryAddressRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;
        private const int MaxAddressesPerUser = 10;

        public DeliveryAddressRepository(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public async Task<IEnumerable<DeliveryAddressDto>> GetUserAddressesAsync(long userId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);
            
            var sql = @"
                SELECT Id, UserId, Label, RecipientName, Phone, Address, 
                       AddressDetail, Latitude, Longitude, IsDefault, CreatedAt
                FROM DeliveryAddresses
                WHERE UserId = @userId AND IsActive = 1
                ORDER BY IsDefault DESC, CreatedAt DESC";

            return await con.QueryAsync<DeliveryAddressDto>(sql, new { userId });
        }

        public async Task<DeliveryAddressDto> GetAddressByIdAsync(long addressId, long userId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);
            
            var sql = @"
                SELECT Id, UserId, Label, RecipientName, Phone, Address, 
                       AddressDetail, Latitude, Longitude, IsDefault, CreatedAt
                FROM DeliveryAddresses
                WHERE Id = @addressId AND UserId = @userId AND IsActive = 1";

            return await con.QueryFirstOrDefaultAsync<DeliveryAddressDto>(sql, new { addressId, userId });
        }

        public async Task<DeliveryAddressDto> CreateAddressAsync(CreateDeliveryAddressRequest request, long userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var count = await _context.DeliveryAddresses
                .Where(a => a.UserId == userId && a.IsActive)
                .CountAsync();

            if (count >= MaxAddressesPerUser)
                throw new BadRequestException(
                    $"You have reached the maximum number of addresses ({MaxAddressesPerUser})."
                );

            var isDefault = request.IsDefault || count == 0;

            if (isDefault)
            {
                await _context.DeliveryAddresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
            }

            var address = new DeliveryAddress
            {
                UserId = userId,
                Label = request.Label ?? "Home",
                RecipientName = request.RecipientName,
                Phone = request.Phone,
                Address = request.Address,
                AddressDetail = request.AddressDetail,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = isDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryAddresses.Add(address);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            return await GetAddressByIdAsync(address.Id, userId);

        }

        public async Task<DeliveryAddressDto> UpdateAddressAsync(long addressId, UpdateDeliveryAddressRequest request, long userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var address = await _context.DeliveryAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);

            if (address == null)
                throw new BadRequestException("Delivery address not found");

            // If setting as default, unset other defaults
            if (request.IsDefault && !address.IsDefault)
            {
                await _context.DeliveryAddresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
            }

            if(!request.IsDefault && address.IsDefault)
            {
                var hasOtherDefault = await _context.DeliveryAddresses
                    .AnyAsync(a => a.UserId == userId && a.IsDefault && a.Id != addressId);

                if (!hasOtherDefault)
                    throw new BadRequestException("At least one default address is required.");
            }

            address.Label = request.Label ?? address.Label;
            address.RecipientName = request.RecipientName;
            address.Phone = request.Phone;
            address.Address = request.Address;
            address.AddressDetail = request.AddressDetail;
            address.Latitude = request.Latitude ?? address.Latitude;
            address.Longitude = request.Longitude ?? address.Longitude;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetAddressByIdAsync(addressId, userId);
        }

        public async Task<bool> DeleteAddressAsync(long addressId, long userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var address = await _context.DeliveryAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);

            if (address == null)
                return false;

            var wasDefault = address.IsDefault;

            address.IsActive = false;
            address.IsDefault = false;
            address.UpdatedAt = DateTime.UtcNow;

            if (wasDefault)
            {
                var nextAddress = await _context.DeliveryAddresses
                    .Where(a => a.UserId == userId && a.IsActive)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextAddress != null)
                    nextAddress.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<DeliveryAddressDto> SetDefaultAddressAsync(long addressId, long userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var address = await _context.DeliveryAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);

            if (address == null)
                throw new BadRequestException("Delivery address not found");

            if (address.IsDefault)
                return await GetAddressByIdAsync(addressId, userId);

            // Unset current default
            await _context.DeliveryAddresses
                .Where(a => a.UserId == userId && a.IsActive && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));

            // Set new default
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetAddressByIdAsync(addressId, userId);
        }

        public async Task<DeliveryAddressDto> GetDefaultAddressAsync(long userId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);
            
            var sql = @"
                SELECT TOP 1 Id, UserId, Label, RecipientName, Phone, Address, 
                       AddressDetail, Latitude, Longitude, IsDefault, CreatedAt
                FROM DeliveryAddresses
                WHERE UserId = @userId AND IsActive = 1
                ORDER BY IsDefault DESC, CreatedAt DESC";

            return await con.QueryFirstOrDefaultAsync<DeliveryAddressDto>(sql, new { userId });
        }
    }
}
