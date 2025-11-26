using MongoDB.Driver;
using SmartQuiz.Models;
using YourProject.Models;

namespace SmartQuiz.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IMongoCollection<Quiz> _quizCollection;
        private readonly IMongoCollection<Tp> _tpCollection;
        private readonly IMongoCollection<User> _userCollection;

        public TeacherService(IConfiguration config)
        {
            // Récupération CORRECTE des valeurs dans appsettings.json
            var connectionString = config["MongoDB:ConnectionString"];
            var databaseName = config["MongoDB:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("❌ MongoDB connection string is missing in appsettings.json!");

            if (string.IsNullOrEmpty(databaseName))
                throw new Exception("❌ MongoDB database name is missing in appsettings.json!");

            // Connexion à MongoDB
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);

            // Collections
            _quizCollection = db.GetCollection<Quiz>("Quizzes");
            _tpCollection = db.GetCollection<Tp>("Tps");
            _userCollection = db.GetCollection<User>("Users");
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

        public async Task<List<User>> GetTeachersByIdsAsync(List<string> teacherIds)
        {
            if (teacherIds == null || teacherIds.Count == 0)
                return new List<User>();

            var objectIds = teacherIds
                .Select(id => MongoDB.Bson.ObjectId.Parse(id))
                .ToList();

            var filter = Builders<User>.Filter.In(u => u.Id, objectIds);

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
            var objectId = MongoDB.Bson.ObjectId.Parse(id);

            return await _userCollection
                .Find(u => u.Id == objectId && u.Role == "Teacher")
                .FirstOrDefaultAsync();
        }
    }
}
