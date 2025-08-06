using MessageApp.Model;

namespace MessageApp.Repositories.Interface
{

    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
    }

}