using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface IChatbotAIService
    {
        Task<string> AskAsync(string message, ClaimsPrincipal user);
        Task<string> AskChatbotAsync(string message, ClaimsPrincipal user);
    }
}

