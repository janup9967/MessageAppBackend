using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace MessageApp.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        // Track connected users and their connection IDs
        private static readonly ConcurrentDictionary<int, List<string>> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                _userConnections.AddOrUpdate(userId.Value,
                    new List<string> { Context.ConnectionId },
                    (key, existing) =>
                    {
                        if (!existing.Contains(Context.ConnectionId))
                            existing.Add(Context.ConnectionId);
                        return existing;
                    });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null && _userConnections.TryGetValue(userId.Value, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (!connections.Any())
                {
                    _userConnections.TryRemove(userId.Value, out _);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int receiverId, object message)
        {
            var senderId = GetUserId();
            if (senderId == null) return;

            // Send to receiver
            if (_userConnections.TryGetValue(receiverId, out var receiverConnections))
            {
                foreach (var connId in receiverConnections)
                {
                    await Clients.Client(connId).SendAsync("ReceiveMessage", message);
                }
            }

            // Also send back to sender (so they see their message instantly)
            if (_userConnections.TryGetValue(senderId.Value, out var senderConnections))
            {
                foreach (var connId in senderConnections)
                {
                    await Clients.Client(connId).SendAsync("ReceiveMessage", message);
                }
            }
        }

        public async Task SendReadReceipt(int messageId, int senderId)
        {
            if (_userConnections.TryGetValue(senderId, out var connections))
            {
                foreach (var connId in connections)
                {
                    await Clients.Client(connId).SendAsync("ReceiveReadReceipt", new { messageId });
                }
            }
        }

        public async Task SendTypingIndicator(int receiverId)
        {
            var senderId = GetUserId();
            if (senderId != null && _userConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connId in connections)
                {
                    await Clients.Client(connId).SendAsync("Typing", new { fromUserId = senderId.Value });
                }
            }
        }

        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : null;
        }
    }
}
