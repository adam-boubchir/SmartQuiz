using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface IResultService
    {
        Task<List<Result>> GetResultsByStudentAndTeacherAsync(string studentId, string teacherId);
        Task<List<Result>> GetAllResultsByTeacherAsync(string teacherId);

    }
}
