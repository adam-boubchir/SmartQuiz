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

        // 🔒 Index unique sur email
        private void CreateIndexes()
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };

            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
        }
        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        // ⭐ Méthode manquante — maintenant implémentée
        public async Task UpdateAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(id));
            await _users.DeleteOneAsync(filter);
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

        // 🔒 Hash sécurisé BCrypt
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ✔️ Vérification mot de passe
        public bool CheckPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }

        // ✅ Récupérer tous les utilisateurs
        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(u => true).ToListAsync();
        }

        // ❌ Supprimer un utilisateur par string id
        public async Task DeleteUser(string id)
        {
            var objectId = ObjectId.Parse(id); // conversion string -> ObjectId
            await _users.DeleteOneAsync(u => u.Id == objectId);
        }

        // ✔️ Activer un utilisateur par string id
        public async Task ActivateUser(string id)
        {
            var objectId = ObjectId.Parse(id); // conversion string -> ObjectId
            var update = Builders<User>.Update.Set(u => u.IsApproved, true);
            await _users.UpdateOneAsync(u => u.Id == objectId, update);
        }
        public async Task<User> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetPendingUsers()
        {
            return await _users.Find(u => u.IsApproved == false).ToListAsync();
        }
        

    }
}
