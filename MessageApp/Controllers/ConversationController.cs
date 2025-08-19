
using MessageApp.Dtos;
using MessageApp.Helpers;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MessageApp.Hubs;



namespace MessageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ConversationsController> _logger;
        private readonly IHubContext<MessageHub> _hubContext;

        public ConversationsController(
            IConversationRepository conversationRepository,
            IUserRepository userRepository,
            ILogger<ConversationsController> logger,
            IHubContext<MessageHub> hubContext)
        {
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpPost("Search&Start")]
        [Authorize]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateConversationDto");
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int createdByUserId))
                {
                    _logger.LogWarning("User ID claim not found or invalid");
                    return Unauthorized("User ID not found in token.");
                }

                // Find receiver by username or email
                User? receiver = await _userRepository.GetUserByUsernameAsync(dto.ReceiverIdentifier)
                                ?? await _userRepository.GetUserByEmailAsync(dto.ReceiverIdentifier);

                if (receiver == null)
                {
                    _logger.LogWarning("Receiver not found with identifier: {Identifier}", dto.ReceiverIdentifier);
                    return NotFound("Receiver not found.");
                }

                if (createdByUserId == receiver.Id)
                {
                    _logger.LogWarning("User tried to create a conversation with themselves");
                    return BadRequest("Cannot create a conversation with yourself.");
                }

                // Try to get existing conversation
                var existingConversation = await _conversationRepository.GetConversationBetweenUsersAsync(createdByUserId, receiver.Id);
                if (existingConversation != null)
                {
                    _logger.LogInformation("Conversation already exists between users {User1} and {User2}", createdByUserId, receiver.Id);
                    return Ok(existingConversation);
                }

                var conversation = new Conversation
                {
                    CreatedByUser = createdByUserId,
                    ReceiveId = receiver.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _conversationRepository.CreateConversationAsync(conversation);
                _logger.LogInformation("Conversation created successfully with ID {Id}", created.Id);

                return CreatedAtAction(nameof(CreateConversation), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating conversation");
                return StatusCode(500, "An error occurred while creating the conversation.");
            }
        }

        [HttpGet("All-Coversations")]
        [Authorize]

        public async Task<IActionResult> GetConversations()

        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid");
                    return Unauthorized("User ID not found in token.");
                }

                var conversations = await _conversationRepository.GetConversationsForUserAsync(userId);

                if (conversations == null || !conversations.Any())
                {
                    _logger.LogInformation("No conversations found for user {UserId}", userId);
                    return NotFound("No conversations found.");
                }

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return StatusCode(500, "An error occurred while retrieving conversations.");
            }
        }

        [HttpGet("with/{identifier}")]
        [Authorize]
        public async Task<IActionResult> GetConversationsWithUser(string identifier)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid");
                    return Unauthorized("User ID not found in token.");
                }

                // Find the other user by username or email
                User? otherUser = await _userRepository.GetUserByUsernameAsync(identifier)
                                ?? await _userRepository.GetUserByEmailAsync(identifier);

                if (otherUser == null)
                {
                    _logger.LogWarning("Target user not found with identifier: {Identifier}", identifier);
                    return NotFound("Target user not found.");
                }

                // Get all messages shared between the logged-in user and the identified user
                var messages = await _conversationRepository.GetMessagesBetweenUsersAsync(userId, otherUser.Id);

                if (messages == null || !messages.Any())
                {
                    return NotFound("No messages found between the users.");
                }

                // Map to MessageReturnDto with usernames and avatars
                var messageDtos = new List<MessageReturnDto>();
                foreach (var msg in messages)
                {
                    var sender = await _userRepository.GetUserByIdAsync(msg.SenderId);
                    var receiver = await _userRepository.GetUserByIdAsync(msg.ReceiveId);
                    messageDtos.Add(new MessageReturnDto
                    {
                        SenderUsername = sender?.Username ?? "",
                        ReceiverUsername = receiver?.Username ?? "",
                        ConversationId = msg.ConversationId,
                        Content = msg.Content,
                        Time = msg.Time,
                        IsRead = msg.IsRead
                    });
                }

                return Ok(messageDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages with user {Identifier}", identifier);
                return StatusCode(500, "An error occurred while retrieving the messages.");
            }
        }



        [HttpPost("mark-read/{conversationId}")]
        public async Task<IActionResult> MarkConversationRead(int conversationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                await _conversationRepository.MarkConversationAsReadAsync(conversationId, userId);

                // 🔔 notify other clients in this conversation
                await _hubContext.Clients.Group(conversationId.ToString())
                    .SendAsync("ConversationRead", new { conversationId, userId });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read", conversationId);
                return StatusCode(500, "An error occurred while marking the conversation as read.");
            }




        }
    }
}