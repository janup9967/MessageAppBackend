using MessageApp.Dtos;
using MessageApp.Helpers;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace MessageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        #region 1. Fields & Constructor

        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, JwtService jwtService, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        #endregion

        #region 2. Authentication Endpoints

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserSignupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var existingUserByEmail = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                    return Conflict(new { Message = "Email already registered." });

                var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(dto.Username);
                if (existingUserByUsername != null)
                    return Conflict(new { Message = "Username already taken." });

                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserAsync(user);

                _logger.LogInformation($"User {user.Email} registered successfully.");

                return Ok(new { Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user signup.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var response = await _userRepository.LoginUserAsync(dto);
                if (response == null)
                {
                    return Unauthorized(new { Message = "Invalid username/email or password." });
                }
                _logger.LogInformation($"User {dto.UsernameOrEmail} logged in successfully.");
                return Ok(new { Message = "Login successful.", data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login.");
                return StatusCode(500, new { Message = "Internal server error." });
            }
        }

        #endregion

        #region 3. User Existence

        [HttpGet("exists")]
        [Authorize]
        public async Task<IActionResult> CheckUserExists([FromQuery] string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return BadRequest(new { Message = "Identifier (email or username) is required." });

            try
            {
                var userByEmail = await _userRepository.GetUserByEmailAsync(identifier);
                var userByUsername = await _userRepository.GetUserByUsernameAsync(identifier);

                var user = userByEmail ?? userByUsername;

                if (user == null)
                    return NotFound(new { Message = "User not found." });

                return Ok(new
                {
                    Message = "User exists.",
                    User = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user existence.");
                return StatusCode(500, new { Message = "Internal server error." });
            }
        }

        #endregion

        #region 4. Logout

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Logout failed: Invalid user ID claim");
                return Unauthorized(new { Message = "Invalid user" });
            }

            _logger.LogInformation("Logout attempt for user ID: {UserId}", userId);
            var result = await _userRepository.LogoutAsync(userId);

            if (result == null || !result.Success)
            {
                _logger.LogWarning("Logout failed for user ID: {UserId}. Reason: {Message}", userId, result?.Message);
                return Unauthorized(new { Message = result?.Message ?? "Logout failed" });
            }

            _logger.LogInformation("User successfully logged out. User ID: {UserId}", userId);
            return Ok(new { Message = result.Message });
        }

        #endregion
    }
}
