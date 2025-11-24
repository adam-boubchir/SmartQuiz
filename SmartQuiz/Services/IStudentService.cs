using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQuiz.Models;

namespace SmartQuuiz.Services
{
    public interface IStudentService
    {
        Task<Student> GetByIdAsync(string id);
        Task<Student> GetByEmailAsync(string email);
        Task<List<Student>> GetAllAsync();

        Task<List<Student>> GetByGroupAsync(string groupId);

        Task CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(string id);

        Task<long> CountByTeacherAsync(string teacherId);
        Task<List<Student>> GetRecentByTeacherAsync(string teacherId, int limit = 5);
    }
}
