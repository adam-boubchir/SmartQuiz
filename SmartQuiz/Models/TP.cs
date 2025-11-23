using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace YourProject.Models
{
    public class Tp
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string FilePath { get; set; } // path or url to TP resource

        [BsonRepresentation(BsonType.ObjectId)]
        public string teacherId { get; set; } // teacherId

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TpSubmission> Submissions { get; set; } = new List<TpSubmission>();
    }

    public class TpSubmission
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string StudentId { get; set; }
        public string FilePath { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public double? Grade { get; set; }
        public string Feedback { get; set; }
    }
}
