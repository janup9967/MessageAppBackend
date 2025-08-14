using MessageApp.Dtos;
using MessageApp.Helpers;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MessageApp.Hubs;



namespace MessageApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<MessagesController> _logger;

        // Only one constructor with all dependencies
        public MessagesController(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IConversationRepository conversationRepository,
            ILogger<MessagesController> logger,
            IHubContext<MessageHub> hubContext)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _conversationRepository = conversationRepository;
            _logger = logger;
            _hubContext = hubContext;
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateMessageDto");
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int senderId))
            {
                _logger.LogWarning("User ID claim not found or invalid");
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var receiver = await _userRepository.GetUserByUsernameAsync(dto.ReceiverIdentifier)
                               ?? await _userRepository.GetUserByEmailAsync(dto.ReceiverIdentifier);

                if (receiver == null)
                {
                    _logger.LogWarning("Receiver not found with identifier: {Identifier}", dto.ReceiverIdentifier);
                    return NotFound("Receiver not found.");
                }

                if (senderId == receiver.Id)
                {
                    _logger.LogWarning("User tried to send a message to themselves");
                    return BadRequest("Cannot send a message to yourself.");
                }

                var conversation = await _conversationRepository.GetConversationBetweenUsersAsync(senderId, receiver.Id);
                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        CreatedByUser = senderId,
                        ReceiveId = receiver.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _conversationRepository.CreateConversationAsync(conversation);
                }

                var message = new Message
                {
                    SenderId = senderId,
                    ReceiveId = receiver.Id,
                    ConversationId = conversation.Id,
                    Content = dto.Content,
                    Time = DateTime.UtcNow,
                    IsRead = false
                };

                var sentMessage = await _messageRepository.SendMessageAsync(message);
                var sender = await _userRepository.GetUserByIdAsync(senderId);

                var messageDto = new MessageReturnDto
                {
                    Id = sentMessage.Id,
                    SenderId = sentMessage.SenderId,
                    ReceiveId = sentMessage.ReceiveId,
                    SenderUsername = sender?.Username ?? "Unknown",
                    ReceiverUsername = receiver.Username,
                    ConversationId = sentMessage.ConversationId,
                    Content = sentMessage.Content,
                    Time = sentMessage.Time,
                    IsRead = sentMessage.IsRead
                };

                await _hubContext.Clients.User(receiver.Id.ToString())
                                .SendAsync("ReceiveMessage", messageDto);

                return Ok(messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "An error occurred while sending the message.");
            }
        }



        [HttpPut("read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkReadDto dto)

        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid");
                    return Unauthorized("User ID not found in token.");
                }

                var updatedMessages = new List<Message>();

                foreach (var messageId in dto.MessageIds)
                {
                    var message = await _messageRepository.GetMessageByIdAsync(messageId);
                    if (message == null)
                    {
                        _logger.LogWarning("Message {MessageId} not found", messageId);
                        continue;
                    }

                    if (message.ReceiveId != userId)
                    {
                        _logger.LogWarning("User {UserId} tried to mark message {MessageId} as read but is not the receiver", userId, messageId);
                        continue;
                    }

                    var updatedMessage = await _messageRepository.MarkMessageAsReadAsync(messageId);
                    updatedMessages.Add(updatedMessage);

                    // Notify sender via SignalR
                    await _hubContext.Clients.User(message.SenderId.ToString())
                        .SendAsync("ReceiveReadReceipt", new { messageId = message.Id });
                }

                return Ok(updatedMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, "An error occurred.");
            }


        }

        [HttpGet("unread/{conversationId}")]
        public async Task<IActionResult> GetUnreadMessagesFromConversation(int conversationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("User ID claim not found or invalid");
                return Unauthorized("User ID not found in token.");
            }

            var unreadMessages = await _messageRepository.GetUnreadMessagesByConversationAsync(userId, conversationId);

            var result = unreadMessages.Select(m => new UnreadMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.SenderUsername,
                ReceiverId = m.ReceiverId,
                ReceiverUsername = m.ReceiverUsername,
                Content = m.Content,
                Time = m.Time,
                IsRead = m.IsRead,
                ConversationId = m.ConversationId
            }).ToList();

            return Ok(result);
        }





    }

}