using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;

namespace SmartQuiz.Services
{
    public interface IStudentAnswerService
    {
        Task SaveAnswerAsync(StudentAnswer answer);
        Task<List<StudentAnswer>> GetByStudentAsync(string studentId);
    }
}
