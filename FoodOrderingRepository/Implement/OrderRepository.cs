using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Constants;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace FoodOrderingRepository.Implement
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;
        private readonly IWalletService _walletService;

        public OrderRepository(
            FoodOrderingContext context, 
            IOptions<ConnectionOption> connectionOption,
            IWalletService walletService)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
            _walletService = walletService;
        }
        
        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, long userId)
        {
            // Validate store exists
            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null)
                throw new BadRequestException(OrderMessages.StoreNotFound);

            // Get all food store items
            var foodStoreIds = request.Items.Select(i => i.FoodStoreId).ToList();
            var foodStores = await _context.FoodStores
                .Include(fs => fs.Food)
                .Include(fs => fs.Size)
                .Where(fs => foodStoreIds.Contains(fs.Id) && fs.StoreId == request.StoreId)
                .ToListAsync();

            if (foodStores.Count != request.Items.Count)
                throw new BadRequestException(OrderMessages.FoodStoreNotFound);

            // Calculate totals
            decimal subTotal = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in request.Items)
            {
                var foodStore = foodStores.First(fs => fs.Id == item.FoodStoreId);
                var itemTotal = foodStore.Price * item.Quantity;
                subTotal += itemTotal;

                orderDetails.Add(new OrderDetail
                {
                    FoodStoreId = item.FoodStoreId,
                    Price = foodStore.Price,
                    Quantity = item.Quantity,
                    Total = itemTotal
                });
            }

            decimal total = subTotal + request.DeliveryFee - request.Discount;

            // Check wallet balance if paying with wallet
            if (request.PaymentMethod == PaymentMethod.Wallet)
            {
                var hasBalance = await _walletService.HasSufficientBalanceAsync(userId, total);
                if (!hasBalance)
                    throw new OutOfWalletAmountException(OrderMessages.InsufficientBalance);
            }

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = request.StoreId,
                PurchaseDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.PaymentMethod == PaymentMethod.CashOnDelivery 
                    ? PaymentStatus.Unpaid 
                    : PaymentStatus.Unpaid,
                SubTotal = subTotal,
                DeliveryFee = request.DeliveryFee,
                Discount = request.Discount,
                Total = total,
                DeliveryAddress = request.DeliveryAddress,
                RecipientPhone = request.RecipientPhone,
                RecipientName = request.RecipientName,
                Note = request.Note
            };

            // Set order ID for details
            foreach (var detail in orderDetails)
            {
                detail.OrderId = order.Id;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                _context.OrderDetails.AddRange(orderDetails);
                await _context.SaveChangesAsync();

                // Process payment if using wallet
                if (request.PaymentMethod == PaymentMethod.Wallet)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user.WalletAmount < total)
                        throw new OutOfWalletAmountException(OrderMessages.InsufficientBalance);

                    user.WalletAmount -= total;
                    order.PaymentStatus = PaymentStatus.Paid;

                    // Create wallet transaction
                    var walletTx = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        TransactionType = TransactionType.Payment,
                        Amount = -total,
                        BalanceBefore = user.WalletAmount + total,
                        BalanceAfter = user.WalletAmount,
                        Status = TransactionStatus.Completed,
                        Description = $"Thanh toán đơn hàng #{order.Id.ToString()[..8]}",
                        ExternalReference = order.Id.ToString(),
                        PaymentMethod = "Wallet",
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    };
                    _context.WalletTransactions.Add(walletTx);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return await GetOrderDetailAsync(order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT o.Id, o.UserId, o.StoreId, o.PurchaseDate, o.Status, 
                       o.PaymentMethod, o.PaymentStatus, o.SubTotal, o.DeliveryFee, 
                       o.Discount, o.Total, o.DeliveryAddress, o.RecipientPhone, 
                       o.RecipientName, o.Note, o.CancelReason, o.ConfirmedAt, 
                       o.CompletedAt, o.CancelledAt,
                       s.Name as StoreName, s.Address as StoreAddress, s.ImageSrc as StoreImage
                FROM Orders o
                JOIN Stores s ON o.StoreId = s.Id
                WHERE o.Id = @orderId";

            return await con.QueryFirstOrDefaultAsync<OrderDto>(sql, new { orderId });
        }

        public async Task<OrderDto> GetOrderDetailAsync(Guid orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return null;

            using var con = new SqlConnection(_connectionOption.FOOD);

            var itemsSql = @"
                SELECT od.OrderId, od.FoodStoreId, od.Price, od.Quantity, od.Total,
                       f.Id as FoodId, f.Name as FoodName, f.ImageSrc as FoodImage,
                       fs.SizeId, sz.Name as SizeName
                FROM OrderDetails od
                JOIN FoodStores fs ON od.FoodStoreId = fs.Id
                JOIN Foods f ON fs.FoodId = f.Id
                LEFT JOIN FoodSizes sz ON fs.SizeId = sz.Id
                WHERE od.OrderId = @orderId";

            var items = await con.QueryAsync<OrderDetailDto>(itemsSql, new { orderId });
            order.Items = items.ToList();
            order.TotalItems = items.Sum(i => i.Quantity);

            return order;
        }

        // Get all orders for a user
        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(long userId, OrderStatus? status = null)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT o.Id, o.UserId, o.StoreId, o.PurchaseDate, o.Status, 
                       o.PaymentMethod, o.PaymentStatus, o.Total,
                       s.Name as StoreName, s.Address as StoreAddress, s.ImageSrc as StoreImage,
                       (SELECT COUNT(*) FROM OrderDetails WHERE OrderId = o.Id) as TotalItems
                FROM Orders o
                JOIN Stores s ON o.StoreId = s.Id
                WHERE o.UserId = @userId" +
                (status.HasValue ? " AND o.Status = @status" : "") +
                " ORDER BY o.PurchaseDate DESC";

            return await con.QueryAsync<OrderDto>(sql, new { userId, status });
        }

        // Get all orders for a store
        public async Task<IEnumerable<OrderDto>> GetStoreOrdersAsync(long storeId, OrderStatus? status = null)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var sql = @"
                SELECT o.Id, o.UserId, o.StoreId, o.PurchaseDate, o.Status, 
                       o.PaymentMethod, o.PaymentStatus, o.Total,
                       o.DeliveryAddress, o.RecipientPhone, o.RecipientName,
                       (SELECT COUNT(*) FROM OrderDetails WHERE OrderId = o.Id) as TotalItems
                FROM Orders o
                WHERE o.StoreId = @storeId" +
                (status.HasValue ? " AND o.Status = @status" : "") +
                " ORDER BY o.PurchaseDate DESC";

            return await con.QueryAsync<OrderDto>(sql, new { storeId, status });
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string reason = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new BadRequestException(OrderMessages.OrderNotFound);

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
                throw new BadRequestException(OrderMessages.InvalidStatus);

            order.Status = newStatus;

            switch (newStatus)
            {
                case OrderStatus.Confirmed:
                    order.ConfirmedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Completed:
                    order.CompletedAt = DateTime.UtcNow;
                    // Mark COD orders as paid when completed
                    if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
                        order.PaymentStatus = PaymentStatus.Paid;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    order.CancelReason = reason;
                    // Refund if already paid
                    if (order.PaymentStatus == PaymentStatus.Paid && 
                        order.PaymentMethod == PaymentMethod.Wallet)
                    {
                        await RefundOrderAsync(order);
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            return await GetOrderDetailAsync(orderId);
        }

        public async Task<OrderDto> CancelOrderAsync(Guid orderId, string reason, long userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new BadRequestException(OrderMessages.OrderNotFound);

            if (order.UserId != userId)
                throw new BadRequestException(OrderMessages.OrderNotFound);

            // Can only cancel Pending or Confirmed orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                throw new BadRequestException(OrderMessages.CannotCancel);

            if (string.IsNullOrWhiteSpace(reason))
                throw new BadRequestException(OrderMessages.CancelReasonRequired);

            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, reason);
        }

        // Process payment for order
        public async Task<bool> ProcessPaymentAsync(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.PaymentStatus == PaymentStatus.Paid)
                return false;

            if (order.PaymentMethod == PaymentMethod.Wallet)
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user.WalletAmount < order.Total)
                    return false;

                user.WalletAmount -= order.Total;
                order.PaymentStatus = PaymentStatus.Paid;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // Refund order to wallet
        private async Task RefundOrderAsync(Order order)
        {
            var user = await _context.Users.FindAsync(order.UserId);
            var balanceBefore = user.WalletAmount;

            user.WalletAmount += order.Total;
            order.PaymentStatus = PaymentStatus.Refunded;

            var walletTx = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = order.UserId,
                TransactionType = TransactionType.Refund,
                Amount = order.Total,
                BalanceBefore = balanceBefore,
                BalanceAfter = user.WalletAmount,
                Status = TransactionStatus.Completed,
                Description = $"Hoàn tiền đơn hàng #{order.Id.ToString()[..8]}",
                ExternalReference = order.Id.ToString(),
                PaymentMethod = "Wallet",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTx);
            await _context.SaveChangesAsync();
        }

        // Validate status transition
        private bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
            return (current, next) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Preparing, OrderStatus.Ready) => true,
                (OrderStatus.Ready, OrderStatus.Delivering) => true,
                (OrderStatus.Delivering, OrderStatus.Completed) => true,
                _ => false
            };
        }

        #region Legacy Methods (Backward Compatibility)

        public async Task<IEnumerable<OrderDto>> GetAllOrderAsync(long userId)
        {
            return await GetUserOrdersAsync(userId);
        }

        public async Task<DetailOrderResponse> GetDetailOrder(Guid orderId)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            string orderQuery = @"
                SELECT Id, UserId, PurchaseDate, Total 
                FROM Orders 
                WHERE Id = @orderId";

            string storeQuery = @"
                SELECT TOP 1 s.Id, s.Name, s.Address
                FROM OrderDetails od 
                JOIN FoodStores fs ON od.FoodStoreId = fs.Id
                JOIN Stores s ON fs.StoreId = s.Id
                WHERE od.OrderId = @orderId";

            string foodQuery = @"
                SELECT od.OrderId, od.FoodStoreId, od.Price, od.Quantity, od.Total,
                       f.Id, f.Name 
                FROM OrderDetails od 
                JOIN FoodStores fs ON od.FoodStoreId = fs.Id 
                JOIN Foods f ON fs.FoodId = f.Id 
                WHERE od.OrderId = @orderId";

            var multiQ = await con.QueryMultipleAsync(
                orderQuery + ";" + storeQuery + ";" + foodQuery, 
                new { orderId });

            var order = await multiQ.ReadFirstOrDefaultAsync<OrderDto>();
            if (order == null) throw new BadRequestException();

            var store = await multiQ.ReadFirstOrDefaultAsync<StoreDto>();
            var orderDetails = multiQ.Read<OrderDetailDto, FoodDto, OrderDetailDto>((od, f) =>
            {
                return od;
            }, splitOn: "Id");

            return new DetailOrderResponse
            {
                Order = order,
                Store = store,
                OrderDetails = orderDetails
            };
        }

        public async Task<int> CreateOrderAsync(Dictionary<string, int> orderDetail, long userId)
        {
            // Legacy implementation - kept for backward compatibility
            int count = 0;
            using var con = new SqlConnection(_connectionOption.FOOD);
            con.Open();

            string walletQuery = "SELECT WalletAmount FROM Users WHERE Id = @userId";
            string foodStoreQuery = "SELECT Id, StoreId, FoodId, Price FROM FoodStores WHERE Id IN @idList";

            using var multiQ = await con.QueryMultipleAsync(
                walletQuery + ";" + foodStoreQuery, 
                new { userId, idList = orderDetail.Keys });

            var walletAmount = await multiQ.ReadFirstOrDefaultAsync<decimal>();
            var foodStores = await multiQ.ReadAsync<FoodStoreDto>();

            if (!foodStores.Any()) throw new BadRequestException();

            var orderTotal = foodStores
                .Select(fs => new { fs.Id, fs.Price, Quantity = orderDetail.GetValueOrDefault(fs.Id.ToString()) })
                .Sum(od => od.Quantity * od.Price);

            if (walletAmount < orderTotal) throw new OutOfWalletAmountException();

            var transaction = con.BeginTransaction();
            try
            {
                var orderId = Guid.NewGuid();
                var storeId = foodStores.First().StoreId;

                string insertOrder = @"
                    INSERT INTO Orders (Id, UserId, StoreId, PurchaseDate, Total, Status, PaymentMethod, PaymentStatus, SubTotal) 
                    VALUES (@orderId, @userId, @storeId, GETDATE(), @orderTotal, 0, 1, 1, @orderTotal);
                    UPDATE Users SET WalletAmount = WalletAmount - @orderTotal WHERE Id = @userId";

                await con.ExecuteAsync(insertOrder, 
                    new { orderId, userId, storeId, orderTotal }, transaction);

                string insertDetail = @"
                    INSERT INTO OrderDetails (OrderId, FoodStoreId, Price, Quantity, Total) 
                    SELECT @orderId, @foodStoreId, Price, @quantity, Price * @quantity 
                    FROM FoodStores WHERE Id = @foodStoreId";

                foreach (var foodStoreId in orderDetail.Keys)
                {
                    count += await con.ExecuteAsync(insertDetail, new
                    {
                        orderId,
                        foodStoreId,
                        quantity = orderDetail.GetValueOrDefault(foodStoreId),
                    }, transaction);
                }

                if (count == orderDetail.Count)
                {
                    await transaction.CommitAsync();
                    return count;
                }

                throw new BadRequestException();
            }
            catch (DbException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}
