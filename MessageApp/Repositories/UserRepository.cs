
using MessageApp.Model;
using MessageApp.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using MessageApp.Dtos;


namespace MessageApp.Repositories
{


    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly MessageApp.Helpers.JwtService _jwtService;
        public UserRepository(AppDbContext context, MessageApp.Helpers.JwtService jwtService)
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
    }

}