using MessageApp.Model;

namespace MessageApp.Repositories.Interface
{

    public interface IMessageRepository
    {
        Task<Message> SendMessageAsync(Message message);
    }

}