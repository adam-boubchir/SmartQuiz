using BCrypt.Net;
using HarfBuzzSharp;
using MongoDB.Driver;
using SmartQuiz.Models;

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

            CreateIndexes();
        }

        // 🔒 Index unique sur email
        private void CreateIndexes()
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };

            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }

        // 🔍 Récupère user via email
        public async Task<User> GetByEmail(string email)
        {
            return await _users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
        }

        // ➕ Crée un utilisateur
        public async Task CreateUser(User user)
        {
            // Hash sécurisé BCrypt
            user.PasswordHash = HashPassword(user.PasswordHash);

            try
            {
                await _users.InsertOneAsync(user);
            }
            catch (MongoWriteException ex)
                when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new Exception("Un utilisateur avec cet email existe déjà.");
            }
        }

        // 🔒 Hash sécurisé BCrypt (automatique : salt inclus)
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ✔️ Vérification BCrypt sécurisée
        public bool CheckPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}
