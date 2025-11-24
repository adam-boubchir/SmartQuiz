using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class StudentAnswer
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string StudentId { get; set; }     // ⭐ Important
        public string QuizId { get; set; }        // ⭐ Important

        public List<string> Answers { get; set; } = new List<string>();
        public double Score { get; set; } = 0;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
