using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodOrderingRepository.Interface
{
    public interface ITenantRepository
    {
        Task<IEnumerable<TenantDto>> GetAllTenantAsync();
        Task<TenantDto> GetTenantByIdAsync(int id);
        Task<int> CreateTenantAsync(TenantRequest tenant);
        Task<bool> UpdateTenantAsync(TenantUpdateRequest tenant);
        Task DeleteTenantAsync(int id);
    }
}
