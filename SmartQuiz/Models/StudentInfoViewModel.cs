namespace SmartQuiz.Models
{
    public class StudentInfoViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string GroupName { get; set; }

        // 🔹 Propriétés nécessaires pour la vue
        public string Filiere { get; set; } = "-";
        public int Annee { get; set; } = 0;
    }

    
}
