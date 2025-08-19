
using MessageApp.Data;
using MessageApp.Dtos;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace MessageApp.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConversationRepository> _logger;

        public ConversationRepository(AppDbContext context, ILogger<ConversationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@CreatedByUser", conversation.CreatedByUser),
                    new SqlParameter("@ReceiveId", conversation.ReceiveId),
                    new SqlParameter("@CreatedAt", conversation.CreatedAt)
                };

                var result = await _context.Conversations
                    .FromSqlRaw("EXEC CreateConversation @CreatedByUser, @ReceiveId, @CreatedAt", parameters)
                    .AsNoTracking()
                    .ToListAsync();

                var createdConversation = result.FirstOrDefault();
                _logger.LogInformation("Conversation created with ID {Id}", createdConversation?.Id);
                return createdConversation!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation via stored procedure");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesBetweenUsersAsync(int user1Id, int user2Id)
        {
            var user1Param = new SqlParameter("@User1Id", user1Id);
            var user2Param = new SqlParameter("@User2Id", user2Id);

            return await _context.Messages
                .FromSqlRaw("EXEC GetMessagesBetweenUsers @User1Id, @User2Id", user1Param, user2Param)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<bool> ConversationExistsAsync(int user1Id, int user2Id)
        {
            try
            {
                var user1Param = new SqlParameter("@User1Id", user1Id);
                var user2Param = new SqlParameter("@User2Id", user2Id);

                var result = await _context.Conversations
                                    .FromSqlRaw("EXEC CheckConversationExists @User1Id, @User2Id", user1Param, user2Param)
                                    .AsNoTracking()
                                    .ToListAsync(); // Materialize the result first

                return result.Any(); // Check if any rows were returned
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if conversation exists via stored procedure");
                throw;
            }
        }


        public async Task<List<ConversationDto>> GetConversationsForUserAsync(int userId)
        {
            try
            {
                return await _context.Conversations
                    .Where(c => c.CreatedByUser == userId || c.ReceiveId == userId)
                    .Select(c => new ConversationDto
                    {
                        Id = c.Id,
                        SenderUsername = c.CreatedByUser == userId ? _context.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefault()
                                                                   : _context.Users.Where(u => u.Id == c.CreatedByUser).Select(u => u.Username).FirstOrDefault(),
                        ReceiverUsername = c.ReceiveId == userId ? _context.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefault()
                                                                 : _context.Users.Where(u => u.Id == c.ReceiveId).Select(u => u.Username).FirstOrDefault(),
                        CreatedAt = c.CreatedAt,
                        LastMessageContent = _context.Messages
                            .Where(m => m.ConversationId == c.Id)
                            .OrderByDescending(m => m.Time)
                            .Select(m => m.Content)
                            .FirstOrDefault(),
                        LastMessageTime = _context.Messages
                            .Where(m => m.ConversationId == c.Id)
                            .OrderByDescending(m => m.Time)
                            .Select(m => m.Time)
                            .FirstOrDefault(),
                        LastMessageIsRead = _context.Messages
                            .Where(m => m.ConversationId == c.Id)
                            .OrderByDescending(m => m.Time)
                            .Select(m => m.IsRead)
                            .FirstOrDefault(),

                        DisplayName = c.CreatedByUser == userId
                                            ? _context.Users.Where(u => u.Id == c.ReceiveId).Select(u => u.Username).FirstOrDefault()
                                            : _context.Users.Where(u => u.Id == c.CreatedByUser).Select(u => u.Username).FirstOrDefault()
                    })

                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversations for user {UserId}", userId);
                throw;
            }
        }



        public async Task<Conversation?> GetConversationBetweenUsersAsync(int user1Id, int user2Id)
        {
            return await _context.Conversations.FirstOrDefaultAsync(c =>
                (c.CreatedByUser == user1Id && c.ReceiveId == user2Id) ||
                (c.CreatedByUser == user2Id && c.ReceiveId == user1Id));
        }

        public async Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int userId)
        {
            return await _context.Conversations
                .Include(c => c.Messages.OrderBy(m => m.Time)) // Chronological order
                .Include(c => c.Creator)
                .Include(c => c.Receiver)
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.CreatedByUser == userId || c.ReceiveId == userId));
        }

        public async Task MarkConversationAsReadAsync(int conversationId, int userId)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId && m.ReceiveId == userId && !m.IsRead)
                    .ToListAsync();

                if (messages.Any())
                {
                    foreach (var msg in messages)
                    {
                        msg.IsRead = true;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Marked {Count} messages as read in conversation {ConversationId} for user {UserId}",
                        messages.Count, conversationId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read for user {UserId}", conversationId, userId);
                throw;
            }
        }



    }

}