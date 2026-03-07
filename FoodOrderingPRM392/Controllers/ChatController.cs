using FoodOrderingCore.Constants;
using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using FoodOrderingPRM392.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        // Send a message (REST fallback, prefer SignalR)
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = ChatMessages.Unauthorized });

            var message = await _chatRepository.SendMessageAsync(userId.Value, request);
            return Ok(new ParentResultResponse { Message = ChatMessages.SendSuccess, Result = message });
        }

        // Get messages in a conversation
        [HttpGet("messages/{otherUserId:long}/{storeId:long}")]
        public async Task<IActionResult> GetMessages(
            long otherUserId, long storeId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = ChatMessages.Unauthorized });

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var messages = await _chatRepository.GetMessagesAsync(userId.Value, otherUserId, storeId, pageNumber, pageSize);
            return Ok(new ParentResultResponse
            {
                Message = ChatMessages.GetMessagesSuccess,
                Result = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Messages = messages
                }
            });
        }

        // Get all conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = ChatMessages.Unauthorized });

            var conversations = await _chatRepository.GetConversationsAsync(userId.Value);
            return Ok(new ParentResultResponse { Message = ChatMessages.GetConversationsSuccess, Result = conversations });
        }

        // Mark messages as read
        [HttpPut("read/{otherUserId:long}/{storeId:long}")]
        public async Task<IActionResult> MarkAsRead(long otherUserId, long storeId)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = ChatMessages.Unauthorized });

            var count = await _chatRepository.MarkAsReadAsync(userId.Value, otherUserId, storeId);
            return Ok(new ParentResultResponse
            {
                Message = ChatMessages.MarkAsReadSuccess,
                Result = new { MarkedCount = count }
            });
        }

        // Get unread count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new ParentResponse { Message = ChatMessages.Unauthorized });

            var count = await _chatRepository.GetUnreadCountAsync(userId.Value);
            return Ok(new ParentResultResponse
            {
                Message = "OK",
                Result = new { UnreadCount = count }
            });
        }
    }
}
