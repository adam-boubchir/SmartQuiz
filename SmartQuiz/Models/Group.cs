using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace YourProject.Models
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Nom du groupe (ex: "Groupe 1", "Informatique A", "Maths 3")
        [BsonElement("name")]
        public string Name { get; set; }

        // Référence vers l'enseignant responsable
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("teacherId")]
        public string TeacherId { get; set; }

        // Liste des étudiants
        [BsonElement("studentIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> StudentIds { get; set; } = new List<string>();

        // Date de création du groupe
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Dernière mise à jour
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Pour activer / désactiver un groupe sans le supprimer
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // 🔹 Nouveau : Filière (ex: "GI", "GE", "Informatique")
        public string Filiere { get; set; }

        // 🔹 Nouveau : Année (1, 2, 3...)
        public int Annee { get; set; }

        public string Subject { get; set; } = "";
    }
}
