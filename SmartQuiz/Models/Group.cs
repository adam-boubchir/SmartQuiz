using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models   // FIX
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("TeacherId")]
        public string TeacherId { get; set; }

        [BsonElement("StudentIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> StudentIds { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("Filiere")]
        public string Filiere { get; set; }

        [BsonElement("Annee")]
        public int Annee { get; set; }

        [BsonElement("Subject")]
        public string Subject { get; set; } = "";
    }
}
