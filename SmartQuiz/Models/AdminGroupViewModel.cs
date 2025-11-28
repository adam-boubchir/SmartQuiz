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

}
