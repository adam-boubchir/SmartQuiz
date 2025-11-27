using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace YourProject.Models
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TeacherId { get; set; }

        public List<string> StudentIds { get; set; } = new List<string>();

        // 🔹 Nouveau : Filière (ex: "GI", "GE", "Informatique")
        public string Filiere { get; set; }

        // 🔹 Nouveau : Année (1, 2, 3...)
        public int Annee { get; set; }

        public string Subject { get; set; } = "";
    }
}
