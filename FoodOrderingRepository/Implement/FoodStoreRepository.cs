using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class FoodStoreRepository : IFoodStoreRepository
    {
        private readonly FoodOrderingContext context;
        private readonly ConnectionOption connectionOption;

        public FoodStoreRepository(FoodOrderingContext context, IOptions<ConnectionOption> connectionOption)
        {
            this.context = context;
            this.connectionOption = connectionOption.Value;
        }

        public async Task<FoodStoreDto> CreateFoodStoreAsync(FoodStoreDto request)
        {
            FoodStoreDto foodStoreDto = null;
            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" INSERT INTO FoodStores (Id, StoreId, FoodId, SizeId, Price, IsAvailable)
                       VALUES (@Id, @StoreId, @FoodId, @SizeId, @Price, 1);
                       SELECT Id, StoreId, FoodId, SizeId, Price, IsAvailable
                       FROM FoodStores WHERE Id = @Id; ";

                object param = new 
                {
                    Id = Guid.NewGuid(),
                    request.StoreId,
                    request.FoodId,
                    request.SizeId,
                    request.Price
                };
                foodStoreDto = await con.QueryFirstOrDefaultAsync<FoodStoreDto>(sql, param);
            }
            return foodStoreDto;
        }

        public async Task<IEnumerable<FoodStoreDto>> GetAllFoodStore(FoodStoreFilterRequest request)
        {
            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT fs.Id, fs.StoreId, fs.FoodId, fs.Price, 
                              f.Id, f.Name, f.FoodTypeId, ft.Name 'FoodTypeName', f.ImageSrc, f.IsAvailable,
                              s.Id, s.Name, fz.Id, fz.Name, fz.SortOrder
                       FROM FoodStores fs JOIN Foods f ON fs.FoodId = f.Id
                            JOIN FoodTypes ft ON f.FoodTypeId = ft.Id 
                            JOIN Stores s ON fs.StoreId = s.Id 
                            JOIN FoodSizes fz ON fs.SizeId = fz.Id
                       WHERE 1=1 ";

                if (!string.IsNullOrEmpty(request.FoodName))
                {
                    sql += " AND f.Name LIKE CONCAT('%', @FoodName, '%') ";
                }

                if (request.FoodTypeId != null)
                {
                    sql += " AND f.FoodTypeId = @FoodTypeId ";
                }

                object param = new { request.FoodName, request.FoodTypeId };

                return await con.QueryAsync<FoodStoreDto, FoodDto, StoreDto, FoodStoreDto>(sql, 
                    (fs, f, s) =>
                    {
                        fs.Food = f;
                        fs.Store = s;
                        return fs;
                    }, param);
            }
        }

        public async Task<IEnumerable<FoodStoreDto>> GetFoodStoresByStoreId(long storeId)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT fs.Id, fs.StoreId, fs.FoodId, fs.SizeId, fs.Price, fs.IsAvailable,
                              fz.Name AS SizeName,
                              f.Id, f.Name, f.FoodTypeId, ft.Name 'FoodTypeName', f.ImageSrc, f.IsAvailable
                       FROM FoodStores fs JOIN Foods f ON fs.FoodId = f.Id
                            JOIN FoodTypes ft ON f.FoodTypeId = ft.Id
                            LEFT JOIN FoodSizes fz ON fs.SizeId = fz.Id
                       WHERE fs.StoreId = @storeId ";

                object param = new { storeId };

                return await con.QueryAsync<FoodStoreDto, FoodDto, FoodStoreDto>(sql,
                    (fs, f) =>
                    {
                        fs.Food = f;
                        return fs;
                    }, param, splitOn: "Id");
            }
        }

        public async Task<bool> UpdateFoodStoreAsync(FoodStoreUpdateRequest request)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                var setClauses = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("Id", request.Id);

                if (request.Price.HasValue)
                {
                    setClauses.Add("Price = @Price");
                    parameters.Add("Price", request.Price.Value);
                }

                if (request.SizeId.HasValue)
                {
                    setClauses.Add("SizeId = @SizeId");
                    parameters.Add("SizeId", request.SizeId.Value);
                }

                if (request.IsAvailable.HasValue)
                {
                    setClauses.Add("IsAvailable = @IsAvailable");
                    parameters.Add("IsAvailable", request.IsAvailable.Value);
                }

                if (setClauses.Count == 0) return false;

                string sql = $" UPDATE FoodStores SET {string.Join(", ", setClauses)} WHERE Id = @Id ";
                int rowsAffected = await con.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteFoodStoreAsync(Guid id)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql = " DELETE FROM FoodStores WHERE Id = @Id ";
                object param = new { Id = id };
                int rowsAffected = await con.ExecuteAsync(sql, param);
                return rowsAffected > 0;
            }
        }
    }
}
