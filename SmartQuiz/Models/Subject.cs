using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartQuiz.Models
{
    public class Subject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }              // Ex: "NoSQL", "IA"
    }
}
