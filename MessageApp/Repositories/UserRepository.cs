
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using MessageApp.Dtos;
using MessageApp.Dtos.Common;
using MessageApp.Helpers;


namespace MessageApp.Repositories
{


    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        public UserRepository(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserResponseDto> LoginUserAsync(UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == loginDto.UsernameOrEmail || u.Username == loginDto.UsernameOrEmail);
            if (user == null)
                return null;

            // Use BCrypt to verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            var token = _jwtService.GenerateToken(user);
            return new UserResponseDto
            {
                Username = user.Username,
                Token = token
            };
        }
        public async Task<ApiResponse<object>> LogoutAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return ApiResponse<object>.ErrorResponse("Invalid user");
            }
            // For JWT, logout is stateless. Add token blacklist logic here if needed.
            return ApiResponse<object>.SuccessResponse(new { }, "Logout successful");
        }
    }

}