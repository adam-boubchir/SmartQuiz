using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SmartQuiz.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // sender and recipient are User.Id (ObjectId as string)
        public string SenderId { get; set; }
        public string RecipientId { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // optional: conversation id to group messages two-ways (sender+recipient sorted)
        public string ConversationId { get; set; }
    }
}
