using MessageApp.Data;
using MessageApp.Dtos;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;



namespace MessageApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(AppDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@SenderId", message.SenderId),
                    new SqlParameter("@ReceiverId", message.ReceiveId),
                    new SqlParameter("@ConversationId", message.ConversationId),
                    new SqlParameter("@Content", message.Content),
                    new SqlParameter("@Time", message.Time),
                    new SqlParameter("@IsRead", message.IsRead)
                };

                var result = await _context.Messages
                    .FromSqlRaw("EXEC SendMessage @SenderId, @ReceiverId, @ConversationId, @Content, @Time, @IsRead", parameters)
                    .AsNoTracking()
                    .ToListAsync(); // Avoid chaining FirstOrDefaultAsync
                var sentMessage = result.FirstOrDefault(); // Do this in memory

                if (sentMessage != null)
                {
                    _logger.LogInformation("Message sent with ID {Id}", sentMessage.Id);
                }
                else
                {
                    _logger.LogWarning("SendMessage returned null");
                }
                return sentMessage!;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message via stored procedure");
                throw;
            }
        }

        public async Task<Message> MarkMessageAsReadAsync(int messageId)
        {
            var param = new SqlParameter("@MessageId", messageId);

            var result = await _context.Messages
                .FromSqlRaw("EXEC MarkMessageAsRead @MessageId", param)
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault();
        }

        // ✅ Get all unread messages for a receiver
        public async Task<List<UnreadMessageDto>> GetUnreadMessagesByConversationAsync(int receiverId, int conversationId)
        {
            var receiverParam = new SqlParameter("@ReceiverId", receiverId);
            var conversationParam = new SqlParameter("@ConversationId", conversationId);

            // Execute stored procedure directly on Messages table
            var messages = await _context.Messages
                .FromSqlRaw("EXEC GetUnreadMessagesByConversation @ReceiverId, @ConversationId",
                    receiverParam, conversationParam)
                .AsNoTracking()
                .ToListAsync(); // materialize immediately (no further LINQ here)

            // Now you can map to DTO (in memory)
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var receiverIds = messages.Select(m => m.ReceiveId).Distinct().ToList();

            var senders = await _context.Users
                .Where(u => senderIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var receivers = await _context.Users
                .Where(u => receiverIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var result = messages.Select(m => new UnreadMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = senders.ContainsKey(m.SenderId) ? senders[m.SenderId] : "",
                ReceiverId = m.ReceiveId,
                ReceiverUsername = receivers.ContainsKey(m.ReceiveId) ? receivers[m.ReceiveId] : "",
                Content = m.Content,
                Time = m.Time,
                IsRead = m.IsRead,
                ConversationId = m.ConversationId
            }).ToList();

            return result;
        }







    }
}