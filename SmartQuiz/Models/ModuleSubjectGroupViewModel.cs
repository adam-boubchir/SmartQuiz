namespace SmartQuiz.Models
{
    public class ModuleSubjectGroupViewModel
    {
        public string SubjectName { get; set; }
        public string GroupName { get; set; }
        public string TeacherName { get; set; }
        public int Hours { get; set; }
    }

    public class CreateModuleViewModel
    {
        public string Name { get; set; }
        public string Filiere { get; set; }
        public int Annee { get; set; }

        // Toutes les lignes (matière + groupe + prof + heures)
        public List<ModuleSubjectGroupViewModel> Rows { get; set; } = new();
    }
}
