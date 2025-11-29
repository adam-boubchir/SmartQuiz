namespace SmartQuiz.Models
{
    public class AdminProfileViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        // Avatar
        public string AvatarPath { get; set; }

        // Statistiques (à adapter à ton projet)
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalGroups { get; set; }

        // Liste des profs par exemple
        public List<TeacherInfo> Teachers { get; set; } = new();
    }

    public class TeacherInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
