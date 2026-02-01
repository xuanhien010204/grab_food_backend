using FoodOrderingCore.Constants;
using FoodOrderingCore.Response;
using FoodOrderingPRM392.Extensions;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId.Value, pageNumber, pageSize, isRead);
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = NotificationMessages.GetSuccess,
                Result = new
                {
                    Notifications = notifications,
                    UnreadCount = unreadCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _notificationRepository.GetUnreadCountAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = ResponseMessages.Success,
                Result = new { UnreadCount = count }
            });
        }

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _notificationRepository.MarkAsReadAsync(id, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = NotificationMessages.NotFound });

            return Ok(new ParentResponse { Message = NotificationMessages.MarkReadSuccess });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var count = await _notificationRepository.MarkAllAsReadAsync(userId.Value);

            return Ok(new ParentResultResponse
            {
                Message = NotificationMessages.MarkAllReadSuccess,
                Result = new { MarkedCount = count }
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _notificationRepository.DeleteNotificationAsync(id, userId.Value);

            if (!result)
                return NotFound(new ParentResponse { Message = NotificationMessages.NotFound });

            return Ok(new ParentResponse { Message = NotificationMessages.DeleteSuccess });
        }
    }
}
