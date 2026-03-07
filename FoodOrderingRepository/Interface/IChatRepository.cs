using FoodOrderingCore.Dto;
using FoodOrderingCore.Request;

namespace FoodOrderingRepository.Interface
{
    public interface IChatRepository
    {
        // Send a message
        Task<ChatMessageDto> SendMessageAsync(long senderId, SendMessageRequest request);

        // Get conversation messages between two users for a store
        Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(long userId, long otherUserId, long storeId, int pageNumber = 1, int pageSize = 50);

        // Get all conversations for a user
        Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(long userId);

        // Mark messages as read
        Task<int> MarkAsReadAsync(long userId, long otherUserId, long storeId);

        // Get unread message count
        Task<int> GetUnreadCountAsync(long userId);
    }
}
