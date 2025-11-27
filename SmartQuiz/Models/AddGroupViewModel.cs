using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models
{
    public class AddGroupViewModel
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Filiere { get; set; } = "";

        [Required]
        [Range(1, 10)]
        public int Annee { get; set; }

        [Required]
        public string TeacherId { get; set; } = "";

        [Required]
        public string Subject { get; set; } = "";

        // Liste des IDs des étudiants sélectionnés
        public List<string> StudentIds { get; set; } = new List<string>();

        // Pour afficher dans le formulaire
        public List<User> Teachers { get; set; } = new();
        public List<User> Students { get; set; } = new();
    }
}
