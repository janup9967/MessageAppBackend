using MessageApp.Data;
using MessageApp.Dtos;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
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
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Conversation created with ID {Id}", conversation.Id);
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw;
            }
        }

        public async Task<bool> ConversationExistsAsync(int user1Id, int user2Id)
        {
            try
            {
                return await _context.Conversations.AnyAsync(c =>
                    (c.CreatedByUser == user1Id && c.ReceiveId == user2Id) ||
                    (c.CreatedByUser == user2Id && c.ReceiveId == user1Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if conversation exists");
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
                            .FirstOrDefault()
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


    }

}