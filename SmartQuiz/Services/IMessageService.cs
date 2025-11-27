using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public interface IMessageService
    {
        Task SendMessageAsync(Message msg);
        Task<List<Message>> GetConversationAsync(string conversationId, int limit = 100);
        Task<List<Message>> GetRecentConversationsForUserAsync(string userId, int limit = 50);
        Task<List<ContactDto>> GetContactsForUserAsync(string userId);
        Task MarkAsReadAsync(string messageId, string userId);
        Task MarkConversationAsReadAsync(string conversationId, string userId); // <-- AJOUT
        string GetConversationId(string a, string b);
    }


    public class ContactDto
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; } // username or email
        public string Role { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public string AvatarPath { get; set; }
    }
}
