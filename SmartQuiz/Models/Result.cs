using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartQuiz.Models
{
    [BsonIgnoreExtraElements]
    public class Result
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string StudentId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TeacherId { get; set; }

        public string SubjectName { get; set; }

        public double QuizScore { get; set; }

        public double TestScore { get; set; }

        public double Progression { get; set; } // en pourcentage
    }
}
