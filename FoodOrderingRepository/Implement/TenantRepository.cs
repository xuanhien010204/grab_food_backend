using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class TenantRepository : ITenantRepository
    {
        public readonly ConnectionOption _connectionOption;
        public TenantRepository(IOptions<ConnectionOption> connectionOption)
        {
            _connectionOption = connectionOption.Value;
        }

        public async Task<int> CreateTenantAsync(TenantRequest tenant)
        {
            int tenantId = 0;
            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                var sql = @"INSERT INTO Tenants (Name, CreateTime, UpdateTime) 
                          VALUES (@Name, @CreateTime, @UpdateTime)";
                object parameters = new { Name = tenant.Name, CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow };
                tenantId = await con.ExecuteAsync(sql, parameters);
            }
            return tenantId;
        }

        public async Task DeleteTenantAsync(int id)
        {
            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                var sql = @"DELETE FROM Tenants WHERE Id = @Id";
                object parameters = new { Id = id };
                await con.ExecuteAsync(sql, parameters);
            }
        }

        public async Task<IEnumerable<TenantDto>> GetAllTenantAsync()
        {
            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                var sql = @"SELECT Id, Name, CreateTime, UpdateTime
                            FROM Tenants";
                var tenants = await con.QueryAsync<TenantDto>(sql);
                return tenants;
            }
        }

        public async Task<TenantDto> GetTenantByIdAsync(int id)
        {
            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                var sql = @"SELECT Id, Name, CreateTime, UpdateTime
                            FROM Tenants WHERE Id = @Id";
                object parameter = new { Id = id };
                var tenants = await con.QueryFirstOrDefaultAsync<TenantDto>(sql, parameter);
                return tenants;
            }
        }

        public async Task<bool> UpdateTenantAsync(TenantUpdateRequest tenant)
        {
            using (var con = new SqlConnection(_connectionOption.FOOD))
            {
                var sql = @"UPDATE Tenants SET Name = @Name, UpdateTime = @UpdateTime WHERE Id = @Id";
                object parameters = new { Name = tenant.Name, UpdateTime = DateTime.UtcNow, Id = tenant.Id };
                var rowsAffected = await con.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }
    }
}