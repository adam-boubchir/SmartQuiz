namespace SmartQuiz.Models
{
    public class EditGroupViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public int Annee { get; set; }
        public string Subject { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Liste des étudiants à afficher
        public List<StudentInfoViewModel> Students { get; set; } = new();
        public List<string> SelectedStudentIds { get; set; } = new();

        // Filtres pour la liste des étudiants
        public string? FilterFiliere { get; set; }
        public int? FilterAnnee { get; set; }
        public List<string> Filieres { get; set; } = new();
        public List<int> Annees { get; set; } = new();
    }
}
