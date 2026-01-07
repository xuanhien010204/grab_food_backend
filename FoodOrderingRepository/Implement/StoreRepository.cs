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
    public class StoreRepository : IStoreRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption connectionOption;

        public StoreRepository (FoodOrderingContext context, IOptions<ConnectionOption> option)
        {
            _context = context;
            connectionOption = option.Value;
        }

        public async Task<IEnumerable<StoreDto>> GetAllFoodStore(StoreFilterRequest request)
        {
            IEnumerable<StoreDto> list = null;

            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, Name, Address, Latitude, Longitude, ImageSrc 
                       FROM Stores ";
                list = await con.QueryAsync<StoreDto>(sql);
            }

            return list;
        }

        public async Task<StoreDto> GetStoreDetail(long id)
        {
            StoreDto store;

            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string storeQuery =
                    @" SELECT Id, Name, Address, Latitude, Longitude, ImageSrc 
                       FROM Stores 
                       WHERE Id = @id ; ";
                string foodStoreQuery =
                    @" SELECT fs.Id, fs.StoreId, fs.FoodId, fs.Price, f.Id, f.Name, f.FoodTypeId, ft.Name 'FoodTypeName', f.IsAvailable, f.ImageSrc 
                       FROM FoodStores fs JOIN Foods f ON fs.FoodId = f.Id
                            JOIN FoodTypes ft ON f.FoodTypeId = ft.Id 
                       WHERE fs.StoreId = @id AND f.IsAvailable = 1 ";

                object param = new { id };

                var multiQ = await con.QueryMultipleAsync(storeQuery + foodStoreQuery, param);

                store = await multiQ.ReadFirstOrDefaultAsync<StoreDto>();

                if (store != null)
                {
                    var foodStoreList = multiQ.Read<FoodStoreDto, FoodDto, FoodStoreDto>((fs, f) =>
                    {
                        fs.Food = f;
                        return fs;
                    }, splitOn: "Id");

                    store.FoodStores = foodStoreList;
                }
            }

            return store;
        }
    }
}
