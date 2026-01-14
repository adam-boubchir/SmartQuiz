using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class ChatConversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }
        public string Role { get; set; }

        public string Title { get; set; } // ex: "Aide quiz réseaux"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatAIMessage> Messages { get; set; } = new();
    }

    public class ChatAIMessage
    {
        public string Sender { get; set; } // "user" | "ai"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
