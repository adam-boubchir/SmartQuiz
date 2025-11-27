using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuiz.Models
{
    public class TeacherResultsViewModel
    {
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }

        // Résultats détaillés (utilisé dans la vue)
        public List<StudentQuizResult> Results { get; set; } = new();

        // Statistiques globales
        public int TotalStudents { get; set; }
        public int TotalQuizzes { get; set; }

        public double AverageScore
        {
            get
            {
                if (Results == null || Results.Count == 0) return 0;
                return Math.Round(Results.Average(r => r.Score), 2);
            }
        }
    }

    public class StudentQuizResult
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        public string QuizId { get; set; }
        public string QuizTitle { get; set; }

        public double Score { get; set; }
        public DateTime DateTaken { get; set; }
    }
}
