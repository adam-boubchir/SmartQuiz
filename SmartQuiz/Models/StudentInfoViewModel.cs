namespace SmartQuiz.Models
{
    public class StudentInfoViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }   // <- ajouté
        public string Email { get; set; }
        public string GroupName { get; set; }
    }
}
