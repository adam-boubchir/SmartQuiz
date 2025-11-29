using System.Collections.Generic;

namespace SmartQuiz.Models;

public class EditStudentViewModel
{
         public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string SelectedGroupId { get; set; }
        public string SelectedFiliere { get; set; }
        public int? SelectedAnnee { get; set; }

        public List<Group> Groups { get; set; }



        public List<string> Filieres { get; set; }
        public List<int> Annees { get; set; }

       
}
