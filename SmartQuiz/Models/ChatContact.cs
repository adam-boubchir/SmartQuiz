using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartQuiz.Models
{
    public class ChatContact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // User id of the contact
        public string UserId { get; set; }

        // Display name (username or email)
        public string DisplayName { get; set; }

        // Role: Admin / Teacher / Student
        public string Role { get; set; }

        // optional avatar url
        public string AvatarPath { get; set; }

        public long LastMessageAtUnix { get; set; } = 0;
    }
}
