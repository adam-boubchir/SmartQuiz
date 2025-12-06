using MongoDB.Driver;
using SmartQuiz.Models;
using YourProject.Models;

namespace SmartQuiz.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Student> _students;
        private readonly IMongoCollection<Module> _modules;
        private readonly IMongoCollection<Quiz> _quizzes;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:DatabaseName"]);

            _users = db.GetCollection<User>("Users");

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }

        // ---------------------------------------------------
        // 🔹 GET BY ID (STRING ONLY — NO ObjectId.Parse)
        // ---------------------------------------------------
        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _users.Find(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        }

        public async Task CreateUser(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            await _users.InsertOneAsync(user);
        }

        public async Task CreateAsync(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }

        // ---------------------------------------------------
        // 🔹 Update BASIC info — NO ObjectId.Parse
        // ---------------------------------------------------
        public async Task<bool> UpdateBasicInfoAsync(string userId, string username, string email)
        {
            var update = Builders<User>.Update
                .Set(u => u.Username, username)
                .Set(u => u.Email, email);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

            return result.ModifiedCount > 0;
        }

        public async Task UpdateProfileImageAsync(string userId, string imageUrl)
        {
            var update = Builders<User>.Update
                .Set(u => u.ProfileImage, imageUrl);

            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task DeleteUser(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        public async Task ActivateUser(string id)
        {
            var update = Builders<User>.Update.Set(u => u.IsApproved, true);
            await _users.UpdateOneAsync(u => u.Id == id, update);
        }

        public async Task<List<User>> GetPendingUsers()
        {
            return await _users.Find(u => !u.IsApproved).ToListAsync();
        }

        // Password helpers
        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool CheckPassword(string enteredPassword, string storedHash) =>
            BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);

        // ---------------------------------------------------
        // 🔹 UPDATE STUDENT INFO — STRING ONLY
        // ---------------------------------------------------
        public async Task UpdateStudentBasicInfoAsync(string id, string fullName, string email, string groupId)
        {
            var update = Builders<User>.Update
                .Set(u => u.Username, fullName)
                .Set(u => u.Email, email)
                .Set(u => u.GroupId, groupId);

            await _users.UpdateOneAsync(u => u.Id == id, update);
        }

        // ---------------------------------------------------
        // 🔹 UPDATE TEACHER INFO — STRING ONLY
        // ---------------------------------------------------
        public async Task UpdateTeacherBasicInfoAsync(string id, string username, string email, List<string> groupIds, List<string> subjectIds, int assignedHours)
        {
            var update = Builders<User>.Update
                .Set(u => u.Username, username)
                .Set(u => u.Email, email)
                .Set(u => u.GroupIds, groupIds)
                .Set(u => u.SubjectIds, subjectIds)
                .Set(u => u.AssignedHours, assignedHours);

            await _users.UpdateOneAsync(u => u.Id == id, update);
        }

        // ---------------------------------------------------
        // ADMIN
        // ---------------------------------------------------
        public async Task<int> GetTotalModulesAsync()
        {
            return (int)await _modules.CountDocumentsAsync(FilterDefinition<Module>.Empty);
        }

        public async Task<int> GetTotalStudentsAsync()
        {
            return (int)await _users.CountDocumentsAsync(u => u.Role == "Student");
        }

        public async Task<int> GetTotalTeachersAsync()
        {
            return (int)await _users.CountDocumentsAsync(u => u.Role == "Teacher");
        }

        public async Task<int> GetTotalQuizzesAsync()
        {
            return (int)await _quizzes.CountDocumentsAsync(FilterDefinition<Quiz>.Empty);
        }

        public async Task<List<Module>> GetModulesAsync(string userId)
        {
            return await _modules.Find(m => m.TeacherIds.Contains(userId)).ToListAsync();
        }
    }
}
