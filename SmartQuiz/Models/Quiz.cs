using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace YourProject.Models
{
    public class Quiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; } // teacherId

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public int Points { get; set; } = 1;
    }
}
