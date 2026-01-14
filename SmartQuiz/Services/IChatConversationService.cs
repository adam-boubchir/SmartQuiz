using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface IChatConversationService
    {
        Task<List<ChatConversation>> GetUserConversationsAsync(string userId);
        Task<ChatConversation> GetByIdAsync(string id);
        Task<ChatConversation> CreateAsync(ChatConversation conversation);
        Task AddMessageAsync(string conversationId, ChatAIMessage message);
    }
}
