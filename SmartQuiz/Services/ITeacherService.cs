using SmartQuiz.Models;
using YourProject.Models;

namespace SmartQuiz.Services
{
    public interface ITeacherService
    {
        int CountQuizzes(string teacherId);
        int CountTps(string teacherId);
        List<Quiz> GetMyQuizzes(string teacherId);
        List<Tp> GetMyTps(string teacherId);

        // AJOUT pour StudentController
       
        Task<User> GetByIdAsync(string id);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetTeachersByIdsAsync(List<string> ids);
    }
}
