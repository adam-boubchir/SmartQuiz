using System.Text.Json;
using System.Text.RegularExpressions;
using SmartQuiz.Services;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class QuizAIService : IQuizAIService
    {
        private readonly IOllamaService _ollama;

        public QuizAIService(IOllamaService ollama)
        {
            _ollama = ollama;
        }

        public async Task<List<Question>> GenerateQuizAsync(string topic, int count, bool multipleChoice)
        {
            string jsonFormat = multipleChoice ?
            @"[
               {""question"":""..."",
                ""choices"":[""opt1"",""opt2"",""opt3"",""opt4""],
                ""correctIndex"":0 }
             ]"
            :
            @"[
               {""question"":""..."",
                ""answer"":""...""}
             ]";

            string prompt = $@"
Génère {count} questions {(multipleChoice ? "à choix multiple" : "à réponse texte")}
sur le sujet : {topic}.

Répond STRICTEMENT en JSON, sans texte autour :
{jsonFormat}
";

            // --- APPEL IA ---
            string raw = await _ollama.GenerateTextAsync(prompt);

            // --- NETTOYAGE JSON ---
            string clean = ExtractJson(raw);

            // --- DESERIALISATION ---
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var parsed = JsonSerializer.Deserialize<List<AIQuizResult>>(clean, options);

            if (parsed == null)
                throw new Exception("Impossible de parser la réponse IA : " + clean);

            // --- TRANSFORMATION ---
            List<Question> questions = new();

            foreach (var q in parsed)
            {
                questions.Add(new Question
                {
                    Text = q.question,
                    Options = q.choices ?? new List<string>(),
                    CorrectIndex = q.correctIndex,
                    Points = 1
                });
            }

            return questions;
        }

        // ----------------------------------------------------
        // 🔥 EXTRACTEUR DE JSON ROBUSTE (Magic Fix)
        // ----------------------------------------------------
        private string ExtractJson(string input)
        {
            // capture le premier bloc JSON présent dans la réponse
            var match = Regex.Match(input, @"\[[\s\S]*\]");
            if (!match.Success)
                throw new Exception("Aucun JSON trouvé dans la réponse IA : " + input);

            return match.Value;
        }
    }
}
