using MessageApp.Data;
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



    }
}