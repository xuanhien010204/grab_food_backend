using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class FoodRepository : IFoodRepository
    {
        private readonly ConnectionOption connectionOption;

        public FoodRepository(IOptions<ConnectionOption> connectionOption)
        {
            this.connectionOption = connectionOption.Value;
        }

        public async Task<FoodDto> CreateFoodAsync(FoodRequest request)
        {
            FoodDto food;
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                var sql = @"INSERT INTO Foods(Name, ImageSrc, FoodTypeId, IsAvailable)
                        VALUES (@Name, @ImageSrc, @FoodTypeId, @IsAvailable);
                        SELECT f.Id, f.Name, f.ImageSrc, f.FoodTypeId, ft.Name AS FoodTypeName, f.IsAvailable
                        FROM Foods f JOIN FoodTypes ft ON f.FoodTypeId = ft.Id
                        WHERE f.Id = SCOPE_IDENTITY();";
                object param = new
                {
                    request.Name,
                    request.ImageSrc,
                    request.FoodTypeId,
                    IsAvailable = true
                };
                food = await con.QueryFirstOrDefaultAsync<FoodDto>(sql, param);
            }
            return food;
        }

        public async Task<IEnumerable<FoodDto>> GetAllFoodAsync()
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @"  SELECT f.Id, f.Name, ImageSrc,FoodTypeId, ft.Name FoodTypeName ,IsAvailable
                        FROM Foods f JOIN FoodTypes ft ON f.FoodTypeId = ft.Id
                        WHERE IsAvailable = 1
                     ";
                return await con.QueryAsync<FoodDto>(sql);
            }
        }

        public async Task<FoodDto> GetFoodByIdAsync(long id)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT f.Id, f.Name, ImageSrc,FoodTypeId, ft.Name FoodTypeName ,IsAvailable
                        FROM Foods f JOIN FoodTypes ft ON f.FoodTypeId = ft.Id
                       WHERE f.Id = @id AND IsAvailable = 1
                     ";
                object param = new { id };
                return await con.QueryFirstOrDefaultAsync<FoodDto>(sql, param); ;
            }
        }

        public async Task<int> UpdateFoodAsync(FoodUpdate update)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                var sql =
                    @" UPDATE Foods SET Name = @Name, ImageSrc = @ImageSrc, FoodTypeId = @FoodTypeId, IsAvailable = @IsAvailable
                       WHERE Id = @Id;
                       SELECT CAST(@Id as int);
                     ";
                return await con.ExecuteScalarAsync<int>(sql, update);
            }
        }
    }
}
