using MongoDB.Bson.Serialization.Attributes;

namespace SmartQuiz.Models
{
    public class ContactDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }

        public string DisplayName { get; set; }
        public string Role { get; set; }

        public string LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public int UnreadCount { get; set; }

        // 🔥 Pour résoudre ton erreur
        public string ProfileImage { get; set; }
    }
}
