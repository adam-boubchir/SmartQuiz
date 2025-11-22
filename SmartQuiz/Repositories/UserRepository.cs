using MongoDB.Driver;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbService mongoDbService)
        {
            _users = mongoDbService.GetCollection<User>("Users");
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task Create(User user)
        {
            await _users.InsertOneAsync(user);
        }
    }
}
