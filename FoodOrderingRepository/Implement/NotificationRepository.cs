using Dapper;
using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;
using FoodOrderingRepository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodOrderingRepository.Implement
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly FoodOrderingContext _context;
        private readonly ConnectionOption _connectionOption;

        public NotificationRepository(
            FoodOrderingContext context,
            IOptions<ConnectionOption> connectionOption)
        {
            _context = context;
            _connectionOption = connectionOption.Value;
        }

        public async Task<NotificationDto> CreateNotificationAsync(
            long userId,
            string title,
            string content,
            NotificationType type,
            string referenceId = null,
            string imageUrl = null,
            string deepLink = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceId = referenceId,
                ImageUrl = imageUrl,
                DeepLink = deepLink,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
            long userId,
            int pageNumber = 1,
            int pageSize = 20,
            bool? isRead = null)
        {
            using var con = new SqlConnection(_connectionOption.FOOD);

            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT Id, UserId, Title, Content, Type, ReferenceId, 
                       ImageUrl, DeepLink, IsRead, ReadAt, CreatedAt
                FROM Notifications
                WHERE UserId = @userId" +
                (isRead.HasValue ? " AND IsRead = @isRead" : "") +
                @" ORDER BY CreatedAt DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            return await con.QueryAsync<NotificationDto>(sql, new { userId, isRead, offset, pageSize });
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, long userId)
        {
            var affected = await _context.Notifications
                .Where(n => n.Id == notificationId && n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow));

            return affected > 0;
        }

        public async Task<int> MarkAllAsReadAsync(long userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow));
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, long userId)
        {
            var affected = await _context.Notifications
                .Where(n => n.Id == notificationId && n.UserId == userId)
                .ExecuteDeleteAsync();

            return affected > 0;
        }

        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task SendOrderStatusNotificationAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return;

            var (title, content) = GetOrderStatusMessage(newStatus, order.Store?.Name ?? "Store");

            await CreateNotificationAsync(
                userId: order.UserId,
                title: title,
                content: content,
                type: NotificationType.Order,
                referenceId: orderId.ToString(),
                imageUrl: order.Store?.ImageSrc,
                deepLink: $"/orders/{orderId}"
            );
        }

        public async Task SendWalletNotificationAsync(long userId, decimal amount, string description)
        {
            var isDeposit = amount > 0;

            var title = isDeposit
                ? "Top-up Successful"
                : "Payment Successful";

            var content = isDeposit
                ? $"You have successfully added {amount:N0} VND to your wallet. {description}"
                : $"You have successfully paid {Math.Abs(amount):N0} VND. {description}";

            await CreateNotificationAsync(
                userId: userId,
                title: title,
                content: content,
                type: NotificationType.Wallet,
                referenceId: null,
                imageUrl: null,
                deepLink: "/wallet"
            );
        }

        private static (string title, string content) GetOrderStatusMessage(OrderStatus status, string storeName)
        {
            return status switch
            {
                OrderStatus.Confirmed => (
                    "Order Confirmed",
                    $"{storeName} has confirmed your order and is preparing it."
                ),
                OrderStatus.Preparing => (
                    "Preparing Your Order",
                    $"{storeName} is preparing your food."
                ),
                OrderStatus.Ready => (
                    "Order Ready",
                    "Your order is ready and waiting for delivery."
                ),
                OrderStatus.Delivering => (
                    "Out for Delivery",
                    "The delivery driver is on the way to you."
                ),
                OrderStatus.Completed => (
                    "Delivery Completed",
                    "Your order has been delivered successfully. Thank you for using our service!"
                ),
                OrderStatus.Cancelled => (
                    "Order Cancelled",
                    "Your order has been cancelled. A refund will be issued if payment was made."
                ),
                _ => (
                    "Order Update",
                    "Your order status has been updated."
                )
            };
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Content = notification.Content,
                Type = notification.Type,
                ReferenceId = notification.ReferenceId,
                ImageUrl = notification.ImageUrl,
                DeepLink = notification.DeepLink,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
