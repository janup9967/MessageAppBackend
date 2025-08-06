
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
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                    return Conflict(new { Message = "Email already registered." });

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
    }
}