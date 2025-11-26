using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class StudentsListViewModel
    {
        public List<StudentInfoViewModel> Students { get; set; } = new List<StudentInfoViewModel>();
        public List<GroupViewModel> AvailableGroups { get; set; } = new List<GroupViewModel>();
    }

    public class GroupViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
