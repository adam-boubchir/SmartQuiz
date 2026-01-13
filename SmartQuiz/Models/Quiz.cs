using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    [BsonIgnoreExtraElements]
    public class Quiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string CreatedBy { get; set; } // teacherId

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        [BsonElement("subjectName")]
        public string SubjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Question> Questions { get; set; } = new List<Question>();
        [BsonIgnore]
        public string Status
        {
            get
            {
                var now = DateTime.UtcNow;

                if (StartDate == null || EndDate == null)
                    return "unknown";

                if (now < StartDate)
                    return "pending";

                if (now > EndDate)
                    return "closed";

                return "open";
            }
        }

    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public int Points { get; set; } = 1;
    }
}
