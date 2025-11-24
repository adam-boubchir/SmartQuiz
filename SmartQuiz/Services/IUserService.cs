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
    }
}
