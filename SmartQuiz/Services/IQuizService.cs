using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;

namespace YourProject.Services
{
    public interface IQuizService
    {
        Task<long> CountByTeacherAsync(string teacherId);
        Task<List<Quiz>> GetRecentByTeacherAsync(string teacherId, int limit = 5);
        Task<List<Quiz>> GetByTeacherAsync(string teacherId);
        Task<Quiz> GetByIdAsync(string id);
        Task CreateAsync(Quiz quiz);
        Task UpdateAsync(Quiz quiz);
        Task DeleteAsync(string id);
    }
}
