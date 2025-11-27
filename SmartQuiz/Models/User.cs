using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
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
        public string Role { get; set; } // Admin, Teacher, Student

        // Student group
        [BsonElement("groupId")]
        public string GroupId { get; set; }

        // Multi teachers
        [BsonElement("teacherIds")]
        public List<string> TeacherIds { get; set; } = new List<string>();

        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = false;

        // Profile image only
        [BsonElement("profileImage")]
        public string ProfileImage { get; set; }

        // Tracking
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
