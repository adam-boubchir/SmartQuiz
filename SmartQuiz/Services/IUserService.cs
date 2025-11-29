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
        Task<bool> UpdateBasicInfoAsync(string userId, string username, string email);

         
        // Admin Profile helpers
        Task<int> GetTotalModulesAsync();
        Task<int> GetTotalStudentsAsync();
        Task<int> GetTotalTeachersAsync();
        Task<int> GetTotalQuizzesAsync();
        Task<List<Module>> GetModulesAsync(string userId);
        
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task UpdateProfileImageAsync(string userId, string imageUrl);

        // password helpers
        string HashPassword(string password);
        bool CheckPassword(string enteredPassword, string storedHash);

        Task UpdateStudentBasicInfoAsync(string id, string fullName, string email, string groupId);

        Task UpdateTeacherBasicInfoAsync(string id, string username, string email, List<string> groupIds, List<string> subjectIds, int assignedHours);
    }
}
