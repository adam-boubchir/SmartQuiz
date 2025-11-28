using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class AdminGroupsViewModel
    {
        public string? SelectedFiliere { get; set; }
        public int? SelectedAnnee { get; set; }

        // 🔹 Nouveau : filtre par professeur (chaîne libre ou liste)
        public string? SelectedTeacher { get; set; }


        public List<string> Filieres { get; set; } = new();
        public List<int> Annees { get; set; } = new();

        // 🔹 Nouveau : nom complet du professeur
        public string TeacherFullName { get; set; }
        
        // 🔹 Optionnel : liste des noms de profs pour un <select>
        public List<string> Teachers { get; set; } = new(); 

        public List<GroupInfoViewModel> Groups { get; set; } = new();
    }

}
