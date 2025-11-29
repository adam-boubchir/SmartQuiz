using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class Student
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        // Un étudiant suit plusieurs profs → matières différents
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> TeacherIds { get; set; } = new List<string>();

        // Un étudiant peut être inscrit dans plusieurs groupes
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> GroupIds { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }   // 🔥 ajout important

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
