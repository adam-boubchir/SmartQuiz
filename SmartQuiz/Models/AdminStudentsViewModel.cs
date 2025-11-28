using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class AdminStudentsViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalGroups { get; set; }
        public int StudentsWithoutGroup { get; set; }
        public int AverageStudentsPerGroup { get; set; }

        public List<StudentInfoViewModel> Students { get; set; }
        public List<GroupViewModel> Groups { get; set; }
    }
}
