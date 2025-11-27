using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class AdminGroupsViewModel
    {
        public string? SelectedFiliere { get; set; }
        public int? SelectedAnnee { get; set; }

        public List<string> Filieres { get; set; } = new();
        public List<int> Annees { get; set; } = new();

        public List<GroupInfoViewModel> Groups { get; set; } = new();
    }

    public class GroupInfoViewModel
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Filiere { get; set; } = "";
        public int Annee { get; set; }
        public int NombreEtudiants { get; set; }
        public string Subject { get; set; } = "";

        // ✅ Propriété calculée pour afficher "1ère année" ou "2ème année"
         public string AnneeFormatee => Annee == 1 ? "1ère année" : Annee + "ème année";
    }

    
}
