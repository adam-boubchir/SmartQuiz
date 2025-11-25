using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } // Admin - Teacher - Student

        // ⭐ Groupe de l'étudiant
        [BsonElement("groupId")]
        public string GroupId { get; set; }

        // ⭐⭐ Liste des enseignants associés (MULTI-TEACHERS)
        [BsonElement("teacherIds")]
        public List<string> TeacherIds { get; set; } = new List<string>();

        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("avatarPath")]
        public string AvatarPath { get; set; }

    }
}
