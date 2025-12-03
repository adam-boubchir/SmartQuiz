using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartQuiz.Models
{
    public class TeacherFullInfoViewModel

{
    
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    public List<GroupInfo> Groups { get; set; } = new List<GroupInfo>();
    public List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();
    public List<QuizInfo> Quizzes { get; set; } = new List<QuizInfo>();

    public int TotalHours { get; set; } = 0;
}

public class GroupInfo
{
    public string Name { get; set; }
    public string Filiere { get; set; }
    public int Annee { get; set; }
    public string Subject { get; set; }
}

public class ModuleInfo
{
    public string Name { get; set; }
    public string Filiere { get; set; }
    public int Annee { get; set; }
    public List<GroupAssignmentInfo> GroupAssignments { get; set; } = new List<GroupAssignmentInfo>();
    public List<ModuleSubjectViewModel> Subjects { get; set; }
}

public class GroupAssignmentInfo
{
    public string GroupName { get; set; }
    public string SubjectName { get; set; }
    public int Hours { get; set; }
}

public class ModuleSubjectViewModel
{
    public string SubjectName { get; set; }
    public List<ModuleGroupAssignment> GroupAssignments { get; set; }
}

public class QuizInfo
{
    public string Title { get; set; }
    public string GroupName { get; set; }
    public string SubjectName { get; set; }
}

}
