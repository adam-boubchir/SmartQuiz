using MongoDB.Driver;
using YourProject.Models;

namespace SmartQuiz.Services
{
    public class TeacherService
    {
        private readonly IMongoCollection<Quiz> _quizCollection;
        private readonly IMongoCollection<Tp> _tpCollection;

        public TeacherService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var db = client.GetDatabase("SmartQuizDb");

            _quizCollection = db.GetCollection<Quiz>("Quizzes");
            _tpCollection = db.GetCollection<Tp>("Tps");
        }

        public int CountQuizzes(string teacherId)
        {
            return (int)_quizCollection.CountDocuments(q => q.CreatedBy == teacherId);
        }

        public int CountTps(string teacherId)
        {
            return (int)_tpCollection.CountDocuments(tp => tp.teacherId == teacherId);
        }

        public List<Quiz> GetMyQuizzes(string teacherId)
        {
            return _quizCollection.Find(q => q.CreatedBy == teacherId).ToList();
        }

        public List<Tp> GetMyTps(string teacherId)
        {
            return _tpCollection.Find(tp => tp.teacherId == teacherId).ToList();
        }
    }
}
