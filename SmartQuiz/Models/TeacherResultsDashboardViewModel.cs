using System.Collections.Generic;

namespace SmartQuiz.Models
{
    public class TeacherResultsDashboardViewModel
    {
        public int TotalEvaluatedStudents { get; set; }
        public double AverageQuiz { get; set; }
        public double AverageTest { get; set; }
        public double AverageProgress { get; set; }

        public List<GroupStats> GroupStats { get; set; }
        public List<StudentScoreSummary> TopStudents { get; set; }
    }

    public class GroupStats
    {
        public string Subject { get; set; }
        public double AverageQuiz { get; set; }
        public double AverageTest { get; set; }
        public double AverageProgress { get; set; }
    }

    public class StudentScoreSummary
    {
        public string StudentId { get; set; }
        public double Average { get; set; }
    }
}
