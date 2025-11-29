using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartQuiz.Models
{
    public class Module
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public string TeacherName { get; set; }
        
        public int Hours { get; set; }
        public List<string> TeacherIds { get; set; } = new List<string>();
    }
}
