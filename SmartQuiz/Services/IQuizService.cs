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

        Task<List<Quiz>> GetByTeachersAsync(List<string> teacherIds);
        Task<Quiz> GetByIdAsync(string id);
        Task<List<Quiz>> GetByGroupAsync(string groupId);

        // ⭐ IMPORTANT POUR LES ÉTUDIANTS
       

        Task CreateAsync(Quiz quiz);
        Task UpdateAsync(Quiz quiz);
        Task DeleteAsync(string id);
    }
}
