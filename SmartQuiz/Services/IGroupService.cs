using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQuiz.Models;   // FIX

namespace SmartQuiz.Services   // FIX 
{
    public interface IGroupService
    {
        Task<List<Group>> GetAllAsync();
        Task<Group> GetByIdAsync(string id);
        Task<List<Group>> GetByTeacherAsync(string teacherId);
        Task<long> CountByTeacherAsync(string teacherId);
        Task<int> CountStudentsByTeacherAsync(string teacherId);
        Task CreateAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(string id);
    }
}
