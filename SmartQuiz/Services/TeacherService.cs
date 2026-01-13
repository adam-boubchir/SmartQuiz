using MongoDB.Driver;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IMongoCollection<Quiz> _quizCollection;
        private readonly IMongoCollection<Tp> _tpCollection;
        private readonly IMongoCollection<User> _userCollection;

        public TeacherService(IConfiguration config)
        {
            var connectionString = config["MongoDB:ConnectionString"];
            var databaseName = config["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);

            _quizCollection = db.GetCollection<Quiz>("Quizzes");
            _tpCollection = db.GetCollection<Tp>("Tps");
            _userCollection = db.GetCollection<User>("Users");
        }

        // -----------------------------------------
        // 🔹 COUNT — STRING IDs
        // -----------------------------------------
        public int CountQuizzes(string teacherId)
        {
            return (int)_quizCollection.CountDocuments(q => q.CreatedBy == teacherId);
        }

        public int CountTps(string teacherId)
        {
            return (int)_tpCollection.CountDocuments(tp => tp.teacherId == teacherId);
        }

        // -----------------------------------------
        // 🔹 GET LISTS
        // -----------------------------------------
        public List<Quiz> GetMyQuizzes(string teacherId)
        {
            return _quizCollection.Find(q => q.CreatedBy == teacherId).ToList();
        }

        public List<Tp> GetMyTps(string teacherId)
        {
            return _tpCollection.Find(tp => tp.teacherId == teacherId).ToList();
        }

        // -----------------------------------------
        // 🔹 GET TEACHERS BY IDS (STRING)
        // -----------------------------------------
        public async Task<List<User>> GetTeachersByIdsAsync(List<string> teacherIds)
        {
            if (teacherIds == null || teacherIds.Count == 0)
                return new List<User>();

            var filter = Builders<User>.Filter.In(u => u.Id, teacherIds);

            return await _userCollection.Find(filter).ToListAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _userCollection
                .Find(u => u.Role == "Teacher")
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _userCollection
                .Find(u => u.Id == id && u.Role == "Teacher")
                .FirstOrDefaultAsync();
        }
    }
}
