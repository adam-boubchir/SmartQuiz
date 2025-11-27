namespace SmartQuiz.Models
{
    public class ContactDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }

        // Ajout des propriétés manquantes utilisées dans la vue :
        public string DisplayName { get; set; }
        public string Role { get; set; }

        public int UnreadCount { get; set; }
    }
}
