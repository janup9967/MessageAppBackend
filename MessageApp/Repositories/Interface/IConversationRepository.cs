using MessageApp.Dtos;
using MessageApp.Model;

namespace MessageApp.Repositories.Interface
{


    public interface IConversationRepository
    {
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task<bool> ConversationExistsAsync(int user1Id, int user2Id);
        Task<List<Message>> GetMessagesBetweenUsersAsync(int user1Id, int user2Id);
        Task<List<ConversationDto>> GetConversationsForUserAsync(int userId);
        Task<Conversation?> GetConversationBetweenUsersAsync(int user1Id, int user2Id);

        Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int userId);
    }

}