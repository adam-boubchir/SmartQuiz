using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    [BsonIgnoreExtraElements]
    public class Subject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("filiere")]
        public string Filiere { get; set; }

        [BsonElement("annee")]
        public string Annee { get; set; }

        // ✅ Ajouter TeacherIds
        [BsonElement("teacherIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> TeacherIds { get; set; } = new List<string>();

        // Optionnel : si tu as les assignments de groupes
        [BsonElement("groupAssignments")]
        public List<GroupAssignment> GroupAssignments { get; set; } = new List<GroupAssignment>();
    }

}
