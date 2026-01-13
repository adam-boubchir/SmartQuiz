using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using SmartQuiz.Models;

[Route("Api")]
public class ApiController : Controller
{
    [HttpPost("AiGenerateQuiz")]
    public async Task<IActionResult> AiGenerateQuiz([FromBody] AiQuizRequest req)
    {
        // Exécuter ollama en ligne de commande :
        var prompt = $"Génère {req.Count} questions {(req.Type == "mcq" ? "à choix multiple" : "à réponse texte")} sur le sujet : {req.Topic}. " +
                     "Format JSON strict : [{\"question\":\"...\",\"choices\":[],\"answer\":\"...\"}]";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ollama",
                Arguments = $"run llama3 \"{prompt}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();

        // Convertir JSON → HTML
        var json = JsonSerializer.Deserialize<List<QuizAiModel>>(output);

        string html = "";
        foreach (var q in json)
        {
            html += $"<p><b>Q:</b> {q.question}</p>";

            if (q.choices != null && q.choices.Count > 0)
            {
                html += "<ul>";
                foreach (var c in q.choices)
                    html += $"<li>{c}</li>";
                html += "</ul>";
            }

            html += $"<p><b>Réponse:</b> {q.answer}</p><hr>";
        }

        return Json(new { quizHtml = html });
    }
}




