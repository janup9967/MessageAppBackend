using MessageApp.Dtos;
using MessageApp.Model;

namespace MessageApp.Repositories.Interface
{

    public interface IMessageRepository
    {
        Task<Message> SendMessageAsync(Message message);
        Task<Message?> GetMessageByIdAsync(int messageId);

        Task<Message> MarkMessageAsReadAsync(int messageId);

        Task<List<UnreadMessageDto>> GetUnreadMessagesByConversationAsync(int receiverId, int conversationId);
    }

}