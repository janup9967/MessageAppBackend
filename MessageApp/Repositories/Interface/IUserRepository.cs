using MessageApp.Model;
using MessageApp.Dtos;

namespace MessageApp.Repositories.Interface
{

    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<UserResponseDto> LoginUserAsync(UserLoginDto loginDto);
    }

}