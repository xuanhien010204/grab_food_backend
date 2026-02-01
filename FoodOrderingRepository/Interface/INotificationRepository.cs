using FoodOrderingCore.Dto;
using FoodOrderingCore.Enum;

namespace FoodOrderingRepository.Interface
{
    public interface INotificationRepository
    {
        Task<NotificationDto> CreateNotificationAsync(
            long userId,
            string title,
            string content,
            NotificationType type,
            string referenceId = null,
            string imageUrl = null,
            string deepLink = null);
        
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
            long userId, 
            int pageNumber = 1, 
            int pageSize = 20,
            bool? isRead = null);
        Task<bool> MarkAsReadAsync(Guid notificationId, long userId);
        Task<int> MarkAllAsReadAsync(long userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, long userId);
        // Get unread count
        Task<int> GetUnreadCountAsync(long userId);
        // Send order status notification
        Task SendOrderStatusNotificationAsync(Guid orderId, OrderStatus newStatus);
        // Send wallet notification
        Task SendWalletNotificationAsync(long userId, decimal amount, string description);
    }
}
