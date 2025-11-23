using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;

namespace YourProject.Services
{
    public interface IGroupService
    {
        Task<long> CountByTeacherAsync(string teacherId);
        Task<int> CountStudentsByTeacherAsync(string teacherId);
        Task<List<Group>> GetByTeacherAsync(string teacherId);
        Task<Group> GetByIdAsync(string id);
        Task CreateAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(string id);
    }
}
