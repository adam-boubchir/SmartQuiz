using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface ISubjectService
    {
        Task<List<Subject>> GetAllAsync();
        Task<Subject> GetByIdAsync(string id);
        Task CreateAsync(Subject subject);
        Task UpdateAsync(string id, Subject subject);
        Task DeleteAsync(string id);
    }
}
