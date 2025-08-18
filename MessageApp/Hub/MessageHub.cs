using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;
using MessageApp.Dtos;

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
                Console.WriteLine($"✅ User {userId} connected with ID {Context.ConnectionId}");

            }
            else
            {
                Console.WriteLine("❌ Connection failed: User ID not found in claims.");
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
                Console.WriteLine($"🔌 User {userId} disconnected from ID {Context.ConnectionId}");

            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(ChatMessageDto message)
        {
            var senderId = GetUserId();
            if (senderId == null)
            {
                Console.WriteLine("❌ SendMessage failed: Sender ID not found.");
                return;

            }
            Console.WriteLine($"📤 Message from {senderId} to {message.ReceiverId}: {message.Content}");


            message.SenderId = senderId.Value;
            message.Timestamp = DateTime.UtcNow;
            message.IsRead = false;

            Console.WriteLine($"📤 Message from {senderId} to {message.ReceiverId}: {message.Content}");
            var connectionsToNotify = new List<string>();


            // Send to receiver
            // if (_userConnections.TryGetValue(message.ReceiverId, out var receiverConnections))
            // {
            //     foreach (var connId in receiverConnections)
            //         await Clients.Client(connId).SendAsync("ReceiveMessage", message);
            // }

            // Send to sender devices
            // if (_userConnections.TryGetValue(senderId.Value, out var senderConnections))
            // {
            //     foreach (var connId in senderConnections)
            //         await Clients.Client(connId).SendAsync("ReceiveMessage", message);
            // }

            // Receiver's devices
            if (_userConnections.TryGetValue(message.ReceiverId, out var receiverConnections))
            {
                connectionsToNotify.AddRange(receiverConnections);
            }

            // Sender's devices
            if (_userConnections.TryGetValue(senderId.Value, out var senderConnections))
            {
                connectionsToNotify.AddRange(senderConnections);
            }

            foreach (var connId in connectionsToNotify.Distinct())
            {
                await Clients.Client(connId).SendAsync("ReceiveMessage", message);
                Console.WriteLine($"📨 Sent message to connection: {connId}");
            }
        }



        public async Task SendReadReceipt(int messageId, int originalSenderId)
        {
            var readerId = GetUserId();
            if (readerId == null)
            {
                Console.WriteLine("❌ SendReadReceipt failed: Reader ID not found.");
                return;
            }
            var connectionsToNotify = new List<string>();

            // Add reader's devices
            if (_userConnections.TryGetValue(readerId.Value, out var readerConnections))
                connectionsToNotify.AddRange(readerConnections);

            // Add original sender's devices
            if (_userConnections.TryGetValue(originalSenderId, out var senderConnections))
                connectionsToNotify.AddRange(senderConnections);

            connectionsToNotify = connectionsToNotify.Distinct().ToList();

            foreach (var connId in connectionsToNotify)
            {
                await Clients.Client(connId).SendAsync("ReceiveReadReceipt", new { messageId });
                Console.WriteLine($"📖 Sent read receipt for message {messageId} to connection: {connId}");
            }

        }

        



        // public async Task SendTypingIndicator(int receiverId)
        // {
        //     var senderId = GetUserId();
        //     if (senderId != null && _userConnections.TryGetValue(receiverId, out var connections))
        //     {
        //         foreach (var connId in connections)
        //         {
        //             await Clients.Client(connId).SendAsync("Typing", new { fromUserId = senderId.Value });
        //         }
        //     }
        // }

        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : null;
        }
    }
}
