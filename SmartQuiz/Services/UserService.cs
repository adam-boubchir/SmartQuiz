using BCrypt.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

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

        public async Task<User> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
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

        public async Task<bool> UpdateBasicInfoAsync(string userId, string username, string email)
        {
            var objectId = ObjectId.Parse(userId);

            var update = Builders<User>.Update
                .Set(u => u.Username, username)
                .Set(u => u.Email, email);

            var result = await _users.UpdateOneAsync(u => u.Id == objectId, update);

            return result.ModifiedCount > 0;
        }

        public async Task UpdateProfileImageAsync(string userId, string imageUrl)
        {
            var objectId = ObjectId.Parse(userId);

            var update = Builders<User>.Update
                .Set(u => u.ProfileImage, imageUrl);

            await _users.UpdateOneAsync(u => u.Id == objectId, update);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task DeleteUser(string id)
        {
            var objectId = ObjectId.Parse(id);
            await _users.DeleteOneAsync(u => u.Id == objectId);
        }

        public async Task ActivateUser(string id)
        {
            var objectId = ObjectId.Parse(id);
            var update = Builders<User>.Update.Set(u => u.IsApproved, true);
            await _users.UpdateOneAsync(u => u.Id == objectId, update);
        }

        public async Task<List<User>> GetPendingUsers()
        {
            return await _users.Find(u => !u.IsApproved).ToListAsync();
        }

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool CheckPassword(string enteredPassword, string storedHash) =>
            BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
    }
}
