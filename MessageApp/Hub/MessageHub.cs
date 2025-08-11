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
                        existing.Add(Context.ConnectionId);
                        return existing;
                    });

                await Clients.Others.SendAsync("UserOnline", userId);
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
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int receiverId, object message)
        {
            if (_userConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connId in connections)
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
            if (_userConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connId in connections)
                {
                    await Clients.Client(connId).SendAsync("Typing", new { fromUserId = GetUserId() });
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
