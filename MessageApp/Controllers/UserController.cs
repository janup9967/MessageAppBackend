
using MessageApp.Dtos;
using MessageApp.Helpers;
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;



namespace MessageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, JwtService jwtService, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }


        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserSignupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                    return Conflict("Email already registered.");

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
    }
}