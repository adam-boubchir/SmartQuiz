using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "Student"; // Admin, Teacher, Student

        // Pour les étudiants
        [BsonElement("groupId")]
        public string? GroupId { get; set; }

        // Pour les enseignants : groupes qu'ils encadrent
        [BsonElement("groupIds")]
        public List<string> GroupIds { get; set; } = new List<string>();

        // Pour les enseignants : matières qu'ils enseignent
        [BsonElement("subjectIds")]
        public List<string> SubjectIds { get; set; } = new List<string>();

        // Heures assignées pour un enseignant
        [BsonElement("assignedHours")]
        public int AssignedHours { get; set; } = 0;

        [BsonElement("teacherIds")]
        public List<string> TeacherIds { get; set; } = new List<string>();

        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("profileImage")]
        public string? ProfileImage { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
