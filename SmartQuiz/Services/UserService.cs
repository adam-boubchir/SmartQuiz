using MongoDB.Driver;
using SmartQuiz.Models;
using System.Security.Cryptography;
using System.Text;

namespace SmartQuiz.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _users = db.GetCollection<User>("Users");
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateUser(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
