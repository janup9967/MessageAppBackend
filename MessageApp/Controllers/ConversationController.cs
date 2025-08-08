
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
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationRepository conversationRepository,
            IUserRepository userRepository,
            ILogger<ConversationsController> logger)
        {
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost]
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

                bool exists = await _conversationRepository.ConversationExistsAsync(createdByUserId, receiver.Id);
                if (exists)
                {
                    _logger.LogInformation("Conversation already exists between users {User1} and {User2}", createdByUserId, receiver.Id);
                    return Conflict("Conversation already exists.");
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

        [HttpGet]
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

    }




}