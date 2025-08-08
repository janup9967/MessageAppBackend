using MessageApp.Data;
using MessageApp.Model;
using MessageApp.Repositories.Interface;

namespace MessageApp.Repositories{
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
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Message sent with ID {Id}", message.Id);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
    }
}

}