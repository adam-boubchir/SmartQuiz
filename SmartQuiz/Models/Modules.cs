using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuiz.Models
{
    public class Module
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Filiere { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string Annee { get; set; }  // ex: "2025"

        public List<ModuleSubject> Subjects { get; set; } = new List<ModuleSubject>();

        public List<string> TeacherIds { get; set; } = new List<string>();
    }

    public class ModuleSubject
    {
        public string SubjectName { get; set; }
        public List<ModuleGroupAssignment> GroupAssignments { get; set; } = new List<ModuleGroupAssignment>();

        [BsonIgnore]
        public int TotalHours => GroupAssignments?.Sum(g => g.Hours) ?? 0;
    }

    public class ModuleGroupAssignment
    {
        public string GroupName { get; set; }
        public string TeacherName { get; set; }
        public int Hours { get; set; } // heures pour ce groupe
    }
}
