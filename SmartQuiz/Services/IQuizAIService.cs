using System.Threading.Tasks;
using System.Collections.Generic;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public interface IQuizAIService
    {
        Task<List<Question>> GenerateQuizAsync(string topic, int count, bool isMultipleChoice);
    }
}
