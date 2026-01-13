namespace SmartQuiz.Models
{
    public class AIQuizResult
    {
        public string question { get; set; }
        public List<string> choices { get; set; }
        public int correctIndex { get; set; }
        public string answer { get; set; }
    }

}
