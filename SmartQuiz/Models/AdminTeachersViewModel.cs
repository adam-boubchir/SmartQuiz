using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartQuiz.Models
{
    public class TeacherInfoViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Subjects { get; set; } = new();
        public int GroupsAssigned { get; set; }
        public int TotalHours { get; set; } // Nombre d'heures assignées
    }

    public class AdminTeachersViewModel
    {
        public string? SelectedSubject { get; set; }
    public List<string> Subjects { get; set; } = new();
    public SelectList? SubjectSelectList { get; set; }

    public SelectList? GroupSelectList { get; set; }
    public string? SelectedGroupId { get; set; }

    public int TotalTeachers { get; set; }
    public int TeachersWithGroups { get; set; }
    public List<TeacherInfoViewModel> Teachers { get; set; } = new();
    }

    public class EditTeacherViewModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    public List<Group> Groups { get; set; } = new();
    public List<string> SelectedGroupIds { get; set; } = new();
    public string NewGroupName { get; set; }

    public List<string> Subjects { get; set; } = new();
    public List<string> SelectedSubjects { get; set; } = new();
    public string NewSubjectName { get; set; }
     public int AssignedHours { get; set; }

}


}
