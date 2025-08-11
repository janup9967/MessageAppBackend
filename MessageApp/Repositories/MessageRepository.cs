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
                _logger.LogInformation("Message sent with ID {Id}", sentMessage?.Id);
                return sentMessage!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message via stored procedure");
                throw;
            }
        }


    }

}