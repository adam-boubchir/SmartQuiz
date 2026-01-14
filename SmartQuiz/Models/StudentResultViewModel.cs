namespace SmartQuiz.Models
{
    public class StudentResultViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string GroupName { get; set; }

        public string QuizId { get; set; }
        public string QuizTitle { get; set; }

        public double Score { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
