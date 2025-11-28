using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class AdminStudentsViewModel
    {
        // 🔹 Filtres
        public string? SelectedFiliere { get; set; }
        public int? SelectedAnnee { get; set; }

        public List<string> Filieres { get; set; } = new();
        public List<int> Annees { get; set; } = new();

        // 🔹 Statistiques
        public int TotalStudents { get; set; }
        public int TotalGroups { get; set; }
        public int StudentsWithoutGroup { get; set; }
        public int AverageStudentsPerGroup { get; set; }

        // 🔹 Listes pour affichage
        public List<StudentInfoViewModel> Students { get; set; } = new();
        public List<GroupViewModel> Groups { get; set; } = new();
    }
}
