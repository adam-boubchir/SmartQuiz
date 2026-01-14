using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class ChatbotAIService : IChatbotAIService
    {
        private readonly IOllamaService _ollamaService;

        public ChatbotAIService(IOllamaService ollamaService)
        {
            _ollamaService = ollamaService;
        }

        // 🔁 MÉTHODE EXISTANTE (UTILISÉE AILLEURS → ON NE TOUCHE PAS)
        public async Task<string> AskAsync(string message, ClaimsPrincipal user)
        {
            string role =
                user.IsInRole("Admin") ? "Administrateur" :
                user.IsInRole("Teacher") ? "Professeur" :
                "Étudiant";

            var prompt = new StringBuilder();
            prompt.AppendLine("Tu es un assistant IA intégré à la plateforme SmartQuiz.");
            prompt.AppendLine($"L'utilisateur est un {role}.");
            prompt.AppendLine("Adapte ton ton, ton niveau technique et tes réponses à ce rôle.");
            prompt.AppendLine("Réponds clairement, précisément et en français.");
            prompt.AppendLine();
            prompt.AppendLine("Question de l'utilisateur :");
            prompt.AppendLine(message);

            string response = await _ollamaService.GenerateTextAsync(prompt.ToString());
            return response.Trim();
        }

        // 🆕 MÉTHODE DÉDIÉE AU CHATBOT (SÉCURISÉE)
        public async Task<string> AskChatbotAsync(string message, ClaimsPrincipal user)
        {
            try
            {
                string role =
                    user.IsInRole("Admin") ? "Administrateur" :
                    user.IsInRole("Teacher") ? "Professeur" :
                    "Étudiant";

                string prompt = $@"
Tu es un assistant pédagogique pour la plateforme SmartQuiz.
Tu aides les utilisateurs selon leur rôle.

Rôle : {role}

Question :
{message}
";

                return (await _ollamaService.GenerateTextAsync(prompt)).Trim();
            }
            catch (Exception ex)
            {
                // 🔒 JAMAIS de crash serveur
                return $"❌ Erreur IA : {ex.Message}";
            }
        }
    }
}
