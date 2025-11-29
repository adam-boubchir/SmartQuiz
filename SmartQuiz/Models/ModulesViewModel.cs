namespace SmartQuiz.Models
{
    public class ModulesViewModel
    {
    public List<ModuleItem> Modules { get; set; }

    public List<Group> AllGroups { get; set; }
    public List<string> AllSubjects { get; set; }

    public string SelectedGroup { get; set; }
    public string SelectedSubject { get; set; }
    }

    public class ModuleItem
    {
    public string Id { get; set; }
    public string GroupName { get; set; }
    public string SubjectName { get; set; }
    public string TeacherName { get; set; }
    public int Hours { get; set; }
    }

}