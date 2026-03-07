using FoodOrderingCore.Constants;
using FoodOrderingCore.Context;
using FoodOrderingCore.Data;
using FoodOrderingCore.Dto;
using FoodOrderingCore.Exceptions;
using FoodOrderingCore.Request;
using FoodOrderingRepository.Interface;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingRepository.Implement
{
    public class ChatRepository : IChatRepository
    {
        private readonly FoodOrderingContext _context;

        public ChatRepository(FoodOrderingContext context)
        {
            _context = context;
        }

        public async Task<ChatMessageDto> SendMessageAsync(long senderId, SendMessageRequest request)
        {
            // Validate receiver exists
            var receiver = await _context.Users.FindAsync(request.ReceiverId);
            if (receiver == null)
                throw new BadRequestException(ChatMessages.ReceiverNotFound);

            // Validate store exists
            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null)
                throw new BadRequestException(ChatMessages.StoreNotFound);

            var sender = await _context.Users.FindAsync(senderId);

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                StoreId = request.StoreId,
                Content = request.Content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = sender?.Name,
                SenderAvatar = sender?.AvatarUrl,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiver.Name,
                StoreId = message.StoreId,
                StoreName = store.Name,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                ReadAt = message.ReadAt
            };
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(long userId, long otherUserId, long storeId, int pageNumber = 1, int pageSize = 50)
        {
            var offset = (pageNumber - 1) * pageSize;

            return await _context.ChatMessages
                .Where(cm => cm.StoreId == storeId &&
                    ((cm.SenderId == userId && cm.ReceiverId == otherUserId) ||
                     (cm.SenderId == otherUserId && cm.ReceiverId == userId)))
                .OrderByDescending(cm => cm.CreatedAt)
                .Skip(offset)
                .Take(pageSize)
                .Select(cm => new ChatMessageDto
                {
                    Id = cm.Id,
                    SenderId = cm.SenderId,
                    SenderName = cm.Sender.Name,
                    SenderAvatar = cm.Sender.AvatarUrl,
                    ReceiverId = cm.ReceiverId,
                    ReceiverName = cm.Receiver.Name,
                    StoreId = cm.StoreId,
                    StoreName = cm.Store.Name,
                    Content = cm.Content,
                    IsRead = cm.IsRead,
                    CreatedAt = cm.CreatedAt,
                    ReadAt = cm.ReadAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(long userId)
        {
            // Get all unique conversations for this user
            var conversations = await _context.ChatMessages
                .Where(cm => cm.SenderId == userId || cm.ReceiverId == userId)
                .GroupBy(cm => new
                {
                    OtherUserId = cm.SenderId == userId ? cm.ReceiverId : cm.SenderId,
                    cm.StoreId
                })
                .Select(g => new
                {
                    g.Key.OtherUserId,
                    g.Key.StoreId,
                    LastMessageAt = g.Max(cm => cm.CreatedAt),
                    UnreadCount = g.Count(cm => cm.ReceiverId == userId && !cm.IsRead)
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            var result = new List<ChatConversationDto>();

            foreach (var conv in conversations)
            {
                var otherUser = await _context.Users.FindAsync(conv.OtherUserId);
                var store = await _context.Stores.FindAsync(conv.StoreId);

                var lastMessage = await _context.ChatMessages
                    .Where(cm => cm.StoreId == conv.StoreId &&
                        ((cm.SenderId == userId && cm.ReceiverId == conv.OtherUserId) ||
                         (cm.SenderId == conv.OtherUserId && cm.ReceiverId == userId)))
                    .OrderByDescending(cm => cm.CreatedAt)
                    .Select(cm => cm.Content)
                    .FirstOrDefaultAsync();

                result.Add(new ChatConversationDto
                {
                    OtherUserId = conv.OtherUserId,
                    OtherUserName = otherUser?.Name,
                    OtherUserAvatar = otherUser?.AvatarUrl,
                    StoreId = conv.StoreId,
                    StoreName = store?.Name,
                    LastMessage = lastMessage,
                    LastMessageAt = conv.LastMessageAt,
                    UnreadCount = conv.UnreadCount
                });
            }

            return result;
        }

        public async Task<int> MarkAsReadAsync(long userId, long otherUserId, long storeId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(cm => cm.ReceiverId == userId &&
                             cm.SenderId == otherUserId &&
                             cm.StoreId == storeId &&
                             !cm.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return unreadMessages.Count;
        }

        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await _context.ChatMessages
                .CountAsync(cm => cm.ReceiverId == userId && !cm.IsRead);
        }
    }
}
