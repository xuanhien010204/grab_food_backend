using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class FoodTypeRepository : IFoodTypeRepository
    {
        private readonly FoodOrderingContext context;
        private readonly ConnectionOption connectionOption;

        public FoodTypeRepository(FoodOrderingContext context, IOptions<ConnectionOption> connectionOption)
        {
            this.context = context;
            this.connectionOption = connectionOption.Value;
        }

        public async Task<int> CreateFoodTypeAsync(FoodTypeCreateRequest foodTypeDto)
        {
            using( var con = new SqlConnection(connectionOption.FOOD))
            {
                var sql =
                    @" INSERT INTO FoodTypes ( Name, ImgSrc )
                       VALUES ( @Name, @ImgSrc );
                       SELECT CAST(SCOPE_IDENTITY() as int);
                     ";
                return await con.ExecuteScalarAsync<int>(sql, foodTypeDto);
            }
        }

        public async Task<bool> DeleteFoodTypeAsync(int id)
        {
            var foodType = await context.FoodTypes.FindAsync(id);
            if (foodType == null)
            {
                return false;
            }
            var hasFood = await context.Foods.AnyAsync(f => f.FoodTypeId == id);
                        if (hasFood)
            {
                return false;
            }
            context.FoodTypes.Remove(foodType);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FoodTypeDto>> GetAllFoodTypeAsync()
        {
            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, Name, ImgSrc 
                       FROM FoodTypes
                     ";
                return await con.QueryAsync<FoodTypeDto>(sql);
            }
        }

        public async Task<FoodTypeDto> GetFoodTypeByIdAsync(int id)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, Name, ImgSrc 
                       FROM FoodTypes
                       WHERE Id = @id
                     ";
                object param = new { id };
                return await con.QueryFirstOrDefaultAsync<FoodTypeDto>(sql, param); ;
            }
        }

        public async Task<int> UpdateFoodTypeAsync(FoodTypeUpdateRequest foodTypeDto)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                var sql =
                    @" UPDATE INTO FoodTypes ( Name, ImgSrc )
                       VALUES ( @Name, @ImgSrc );
                       SELECT CAST(SCOPE_IDENTITY() as int);
                     ";
                return await con.ExecuteScalarAsync<int>(sql, foodTypeDto);
            }
        }
    }
}
