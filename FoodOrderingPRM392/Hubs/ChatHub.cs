using FoodOrderingCore.Dto;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using FoodOrderingPRM392.Extensions;

namespace FoodOrderingPRM392.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;

        // Map userId -> connectionId(s)
        private static readonly Dictionary<long, HashSet<string>> _userConnections = new();
        private static readonly object _lock = new();

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();
            if (userId != null)
            {
                lock (_lock)
                {
                    if (!_userConnections.ContainsKey(userId.Value))
                        _userConnections[userId.Value] = new HashSet<string>();
                    _userConnections[userId.Value].Add(Context.ConnectionId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.GetUserId();
            if (userId != null)
            {
                lock (_lock)
                {
                    if (_userConnections.ContainsKey(userId.Value))
                    {
                        _userConnections[userId.Value].Remove(Context.ConnectionId);
                        if (_userConnections[userId.Value].Count == 0)
                            _userConnections.Remove(userId.Value);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Send message via SignalR
        public async Task SendMessage(long receiverId, long storeId, string content)
        {
            var senderId = Context.User?.GetUserId();
            if (senderId == null) return;

            var request = new FoodOrderingCore.Request.SendMessageRequest
            {
                ReceiverId = receiverId,
                StoreId = storeId,
                Content = content
            };

            var message = await _chatRepository.SendMessageAsync(senderId.Value, request);

            // Send to receiver if online
            HashSet<string> receiverConnections = null;
            lock (_lock)
            {
                if (_userConnections.ContainsKey(receiverId))
                    receiverConnections = new HashSet<string>(_userConnections[receiverId]);
            }

            if (receiverConnections != null)
            {
                foreach (var connId in receiverConnections)
                {
                    await Clients.Client(connId).SendAsync("ReceiveMessage", message);
                }
            }

            // Also send back to sender (confirmation)
            await Clients.Caller.SendAsync("MessageSent", message);
        }

        // Mark messages as read and notify sender
        public async Task MarkAsRead(long otherUserId, long storeId)
        {
            var userId = Context.User?.GetUserId();
            if (userId == null) return;

            var count = await _chatRepository.MarkAsReadAsync(userId.Value, otherUserId, storeId);

            // Notify the other user that messages have been read
            HashSet<string> otherConnections = null;
            lock (_lock)
            {
                if (_userConnections.ContainsKey(otherUserId))
                    otherConnections = new HashSet<string>(_userConnections[otherUserId]);
            }

            if (otherConnections != null)
            {
                foreach (var connId in otherConnections)
                {
                    await Clients.Client(connId).SendAsync("MessagesRead", new
                    {
                        ReadByUserId = userId.Value,
                        StoreId = storeId,
                        Count = count
                    });
                }
            }
        }

        // Get connection IDs for a user (helper for external use)
        public static IEnumerable<string> GetConnectionIds(long userId)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(userId))
                    return new HashSet<string>(_userConnections[userId]);
                return Enumerable.Empty<string>();
            }
        }
    }
}
