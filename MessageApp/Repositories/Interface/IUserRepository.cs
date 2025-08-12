using MessageApp.Model;
using MessageApp.Dtos;
using MessageApp.Dtos.Common;

namespace MessageApp.Repositories.Interface
{

    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int id);
        Task<UserResponseDto> LoginUserAsync(UserLoginDto loginDto);
        Task<ApiResponse<object>> LogoutAsync(string userId);
    }

}