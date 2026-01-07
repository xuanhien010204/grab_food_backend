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

        public async Task<IEnumerable<FoodStoreDto>> GetAllFoodStore(FoodStoreFilterRequest request)
        {
            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT fs.Id, fs.StoreId, fs.FoodId, fs.Price, 
                              f.Id, f.Name, f.FoodTypeId, ft.Name 'FoodTypeName', f.ImageSrc, f.IsAvailable,
                              s.Id, s.Name
                       FROM FoodStores fs JOIN Foods f ON fs.FoodId = f.Id
                            JOIN FoodTypes ft ON f.FoodTypeId = ft.Id 
                            JOIN Stores s ON fs.StoreId = s.Id 
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
    }
}
