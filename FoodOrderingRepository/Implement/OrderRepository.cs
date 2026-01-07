using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace FoodOrderingRepository.Implement
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FoodOrderingContext _context;

        private readonly ConnectionOption connectionOption;

        public OrderRepository(FoodOrderingContext context, IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            this.connectionOption = connectionOption.Value;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrderAsync(long userId)
        {
            IEnumerable<OrderDto> list = null;

            using(var con = new SqlConnection(connectionOption.FOOD))
            {
                string sql =
                    @" SELECT Id, UserId, PurchaseDate, Total 
                       FROM Orders 
                       WHERE UserId = @userId ";

                list = await con.QueryAsync<OrderDto>(sql, new { userId });
            }

            return list.OrderByDescending(o => o.PurchaseDate);
        }

        public async Task<DetailOrderResponse> GetDetailOrder(Guid orderId)
        {
            using (var con = new SqlConnection(connectionOption.FOOD))
            {
                string orderQuery =
                    @" SELECT Id, UserId, PurchaseDate, Total 
                       FROM Orders 
                       WHERE Id = @orderId ; ";
                string storeQuery =
                    @" SELECT TOP 1 s.Id, s.Name, s.Address
                       FROM OrderDetails od JOIN FoodStores fs ON od.FoodStoreId = fs.Id
                            JOIN Stores s ON fs.StoreId = s.Id
                       WHERE od.OrderId = @orderId ; ";
                string foodQuery =
                    @" SELECT od.OrderId, od.FoodStoreId, od.Price, od.Quantity, od.Total,
                              f.Id, f.Name 
                       FROM OrderDetails od JOIN FoodStores fs ON od.FoodStoreId = fs.Id 
                            JOIN Foods f ON fs.FoodId = f.Id 
                       WHERE od.OrderId = @orderId ";

                var multiQ = await con.QueryMultipleAsync(orderQuery + storeQuery + foodQuery, new { orderId });
                OrderDto order = await multiQ.ReadFirstOrDefaultAsync<OrderDto>();

                if (order == null) throw new BadRequestException();

                StoreDto store = await multiQ.ReadFirstOrDefaultAsync<StoreDto>();

                IEnumerable<OrderDetailDto> orderDetails = multiQ.Read<OrderDetailDto, FoodDto, OrderDetailDto>((od, f) =>
                {
                    od.Food = f;
                    return od;
                });

                return new DetailOrderResponse
                {
                    Order = order,
                    Store = store,
                    OrderDetails = orderDetails
                };
            }
        }
        public async Task<int> CreateOrderAsync(Dictionary<string, int> orderDetail, long userId)
        {
            int count = 0;

            using(SqlConnection con = new SqlConnection(connectionOption.FOOD))
            {
                con.Open();
                IEnumerable<FoodStoreDto> foodStore;
                decimal walletAmount;

                string walletQuery =
                    @" SELECT WalletAmount 
                       FROM Users 
                       WHERE Id = @userId ; ";

                string foodStoreQuery =  
                    @" SELECT Id, StoreId, FoodId, Price 
                       FROM FoodStores
                       WHERE Id IN @idList ";

                using (var multiQ = await con.QueryMultipleAsync(walletQuery + foodStoreQuery, new { userId, idList = orderDetail.Keys }))
                {
                    walletAmount = await multiQ.ReadFirstOrDefaultAsync<decimal>();

                    foodStore = await multiQ.ReadAsync<FoodStoreDto>();

                    if (!foodStore.Any()) throw new BadRequestException();
                }

                decimal orderTotal = foodStore.Select(fs => new { fs.Id, fs.Price, Quantity = orderDetail.GetValueOrDefault(fs.Id.ToString()) })
                                              .Sum(od => od.Quantity * od.Price);

                if (walletAmount < orderTotal) throw new OutOfWalletAmountException();

                var transaction = con.BeginTransaction();

                try
                {
                    Guid orderId = Guid.NewGuid();
                    string insertOrder =
                        @" INSERT INTO Orders (Id, UserId, PurchaseDate, Total) 
                           VALUES (@orderId, @userId, GETDATE(), @orderTotal) ;
                           
                           UPDATE Users
                           SET WalletAmount = WalletAmount - @orderTotal
                           WHERE Id = @userId ; ";
                    object param = new { orderId, userId, orderTotal };
                    await con.ExecuteAsync(insertOrder, param, transaction);

                    string insertDetail = 
                        @" INSERT INTO OrderDetails (OrderId, FoodStoreId, Price, Quantity, Total) 
                           SELECT @orderId, @foodStoreId, Price, @quantity, Price*@quantity 
                           FROM FoodStores
                           WHERE Id = @foodStoreId ";

                    foreach (string foodStoreId in orderDetail.Keys)
                    {
                        param = new
                        {
                            orderId,
                            foodStoreId,
                            quantity = orderDetail.GetValueOrDefault(foodStoreId),
                        };
                        count += await con.ExecuteAsync(insertDetail, param, transaction);
                    }

                    if (count == orderDetail.Count)
                    {
                        await transaction.CommitAsync();
                        return count;
                    }

                    throw new BadRequestException();
                }
                catch(DbException)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                catch(Exception)
                {
                    throw new BadRequestException();
                }
            }
        }
    }
}
