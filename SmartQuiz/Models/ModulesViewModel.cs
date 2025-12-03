using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class ModulesViewModel
    {
        public List<ModuleRowViewModel> Modules { get; set; } = new List<ModuleRowViewModel>();
        
    }

    public class ModuleRowViewModel
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Filiere { get; set; }
        public string Annee { get; set; }

        public string SubjectName { get; set; }
        public string GroupName { get; set; }
        public string TeacherName { get; set; }

        public int GroupHours { get; set; }        // heures de ce groupe
        public int TotalSubjectHours { get; set; } // total matière
        public int TotalModuleHours { get; set; }  // total module
    }
}
