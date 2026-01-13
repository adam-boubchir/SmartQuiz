using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;

namespace SmartQuiz.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizAIController : ControllerBase
    {
        private readonly IQuizAIService _quizAI;

        public QuizAIController(IQuizAIService quizAI)
        {
            _quizAI = quizAI;
        }

        [HttpPost("ai")]
        public async Task<IActionResult> GenerateQuiz([FromBody] AIQuizRequest request)
        {
            try
            {
                var questions = await _quizAI.GenerateQuizAsync(
                    request.Topic,
                    request.Count,
                    request.MultipleChoice
                );

                var quiz = new Quiz
                {
                    Title = "Quiz IA : " + request.Topic,
                    Questions = questions
                };

                return Ok(quiz); // ✅ RETOURNE UN VRAI Quiz
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class AIQuizRequest
    {
        public string Topic { get; set; }
        public int Count { get; set; }
        public bool MultipleChoice { get; set; }
    }
}
