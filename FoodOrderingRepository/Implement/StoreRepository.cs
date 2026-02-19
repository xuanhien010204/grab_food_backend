using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ConnectionOption connectionOption;

        public StoreRepository (IOptions<ConnectionOption> option)
        {
            connectionOption = option.Value;
        }

        public async Task<IEnumerable<StoreDto>> GetAllFoodStore()
        {
            IEnumerable<StoreDto> list = null;

            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, TenantId, Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, ManagerId, IsApproved 
                       FROM Stores
                       WHERE IsApproved = 1 ";
                list = await con.QueryAsync<StoreDto>(sql);
            }

            return list;
        }

        public async Task<IEnumerable<StoreDto>> GetAllFoodStoreByTenant(int tenantId)
        {
            IEnumerable<StoreDto> list = null;

            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, TenantId, Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, ManagerId, IsApproved 
                       FROM Stores 
                       WHERE TenantId = @tenantId AND IsApproved = 1 ";
                object param = new { tenantId };
                list = await con.QueryAsync<StoreDto>(sql, param);
            }

            return list;
        }

        public async Task<StoreDto> GetStoreDetail(long id)
        {
            StoreDto store;

            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string storeQuery =
                    @" SELECT Id, TenantId, Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, ManagerId, IsApproved 
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

        public async Task<StoreDto> GetStoreByManagerId(long managerId)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, TenantId, Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, ManagerId, IsApproved 
                       FROM Stores 
                       WHERE ManagerId = @managerId ";
                object param = new { managerId };
                return await con.QueryFirstOrDefaultAsync<StoreDto>(sql, param);
            }
        }

        public async Task<StoreDto> CreateStoreAsync(RegisterManagerRequest request, long managerId)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" INSERT INTO Stores (Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, IsApproved, ManagerId, TenantId, Rating, ReviewCount, MinOrderAmount, DeliveryFee, EstimatedDeliveryTime)
                       VALUES (@Name, @Description, @Address, @Latitude, @Longitude, @ImageSrc, @Phone, @OpenTime, @CloseTime, 1, 1, 0, @ManagerId, 1, 0, 0, 0, 0, 30);
                       SELECT Id, TenantId, Name, Description, Address, Latitude, Longitude, ImageSrc, Phone, OpenTime, CloseTime, IsOpen, IsActive, ManagerId, IsApproved
                       FROM Stores WHERE Id = SCOPE_IDENTITY(); ";

                object param = new
                {
                    request.StoreName,
                    Name = request.StoreName,
                    request.Description,
                    request.Address,
                    request.Latitude,
                    request.Longitude,
                    request.ImageSrc,
                    request.Phone,
                    request.OpenTime,
                    request.CloseTime,
                    ManagerId = managerId
                };

                return await con.QueryFirstOrDefaultAsync<StoreDto>(sql, param);
            }
        }

        public async Task<bool> ApproveStoreAsync(long storeId)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                // Approve store and update user role to Manager
                string sql =
                    @" UPDATE Stores SET IsApproved = 1 WHERE Id = @StoreId;
                       UPDATE Users SET RoleId = 2 WHERE Id = (SELECT ManagerId FROM Stores WHERE Id = @StoreId); ";

                object param = new { StoreId = storeId };
                int rowsAffected = await con.ExecuteAsync(sql, param);
                return rowsAffected > 0;
            }
        }

        public async Task<IEnumerable<StoreDto>> GetPendingStores()
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT s.Id, s.TenantId, s.Name, s.Description, s.Address, s.Latitude, s.Longitude, s.ImageSrc, s.Phone, s.OpenTime, s.CloseTime, s.IsOpen, s.IsActive, s.ManagerId, s.IsApproved
                       FROM Stores s
                       WHERE s.IsApproved = 0 ";

                return await con.QueryAsync<StoreDto>(sql);
            }
        }
    }
}
