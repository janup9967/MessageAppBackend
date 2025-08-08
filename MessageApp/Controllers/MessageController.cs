
using MessageApp.Dtos;
using MessageApp.Helpers;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



namespace MessageApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IConversationRepository conversationRepository,
            ILogger<MessagesController> logger)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _conversationRepository = conversationRepository;
            _logger = logger;
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
                return Ok(sentMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "An error occurred while sending the message.");
            }
        }
    }

}