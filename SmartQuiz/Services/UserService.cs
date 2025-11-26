using BCrypt.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartQuiz.Models;
using YourProject.Models;
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

        // Index email unique
        private void CreateIndexes()
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }

        // GET user by ID
        public async Task<User> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
        }

        // GET user by Email
        public async Task<User> GetByEmail(string email)
        {
            return await _users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
        }

        // Create USER (legacy name)
        public async Task CreateUser(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            await _users.InsertOneAsync(user);
        }

        // CreateAsync (from interface)
        public async Task CreateAsync(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            await _users.InsertOneAsync(user);
        }

        // UPDATE full user
        public async Task UpdateAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }

        // Update basic info: username + email
        public async Task<bool> UpdateBasicInfoAsync(string userId, string username, string email)
        {
            var objectId = ObjectId.Parse(userId);

            var update = Builders<User>.Update
                .Set(u => u.Username, username)
                .Set(u => u.Email, email);

            var result = await _users.UpdateOneAsync(u => u.Id == objectId, update);

            return result.ModifiedCount > 0;
        }

        // Update profile image
        public async Task UpdateProfileImageAsync(string userId, string imageUrl)
        {
            var objectId = ObjectId.Parse(userId);

            var update = Builders<User>.Update
                .Set(u => u.ProfileImage, imageUrl);

            await _users.UpdateOneAsync(u => u.Id == objectId, update);
        }

        // Get ALL users
        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        // Delete user
        public async Task DeleteUser(string id)
        {
            var objectId = ObjectId.Parse(id);
            await _users.DeleteOneAsync(u => u.Id == objectId);
        }

        // Activate user
        public async Task ActivateUser(string id)
        {
            var objectId = ObjectId.Parse(id);
            var update = Builders<User>.Update.Set(u => u.IsApproved, true);
            await _users.UpdateOneAsync(u => u.Id == objectId, update);
        }

        // Pending users
        public async Task<List<User>> GetPendingUsers()
        {
            return await _users.Find(u => u.IsApproved == false).ToListAsync();
        }

        // Password helpers
        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool CheckPassword(string enteredPassword, string storedHash) =>
            BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);


        
        
       
        
    }
}
