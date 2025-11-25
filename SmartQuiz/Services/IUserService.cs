using SmartQuiz.Models;
using System.Threading.Tasks;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmail(string email);
        Task CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task DeleteUser(string id);
        Task ActivateUser(string id);
        Task<List<User>> GetPendingUsers();

        Task CreateAsync(User user);
        Task UpdateAsync(User user);

        // password helpers
        string HashPassword(string password);
        bool CheckPassword(string enteredPassword, string storedHash);
    }
}
