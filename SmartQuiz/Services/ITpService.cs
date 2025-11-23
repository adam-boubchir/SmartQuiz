using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;

namespace YourProject.Services
{
    public interface ITpService
    {
        Task<long> CountByTeacherAsync(string teacherId);
        Task<List<Tp>> GetRecentByTeacherAsync(string teacherId, int limit = 5);
        Task<List<Tp>> GetByTeacherAsync(string teacherId);
        Task<Tp> GetByIdAsync(string id);
        Task CreateAsync(Tp tp);
        Task UpdateAsync(Tp tp);
        Task DeleteAsync(string id);
    }
}
