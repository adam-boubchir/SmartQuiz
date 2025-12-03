namespace SmartQuiz.Models
{
    public class EditTeacherModulesViewModel
{
    public string TeacherId { get; set; }
    public List<ModuleEditViewModel> Modules { get; set; } = new List<ModuleEditViewModel>();
}

public class ModuleEditViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Filiere { get; set; }
    public int Annee { get; set; }

    public List<ModuleSubjectEditViewModel> Subjects { get; set; } = new List<ModuleSubjectEditViewModel>();
}

public class ModuleSubjectEditViewModel
{
    public string SubjectName { get; set; }
    public List<ModuleGroupAssignmentEditViewModel> GroupAssignments { get; set; } = new List<ModuleGroupAssignmentEditViewModel>();
}

public class ModuleGroupAssignmentEditViewModel
{
    public string GroupName { get; set; }
    public int Hours { get; set; }
}

}