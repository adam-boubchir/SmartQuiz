using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartQuiz.Models;   // FIX

namespace SmartQuiz.Services   // FIX
{
    public class GroupService : IGroupService
    {
        private readonly IMongoCollection<Group> _collection;

        public GroupService(MongoDbService mongo)
        {
            _collection = mongo.GetCollection<Group>("Groups");
        }

        public async Task<List<Group>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<long> CountByTeacherAsync(string teacherId)
        {
            var filter = Builders<Group>.Filter.Eq(g => g.TeacherId, teacherId);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> CountStudentsByTeacherAsync(string teacherId)
        {
            var filter = Builders<Group>.Filter.Eq(g => g.TeacherId, teacherId);
            var groups = await _collection.Find(filter).ToListAsync();
            return groups.Sum(g => g.StudentIds?.Count ?? 0);
        }

        public async Task<List<Group>> GetByTeacherAsync(string teacherId)
        {
            var filter = Builders<Group>.Filter.Eq(g => g.TeacherId, teacherId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<Group> GetByIdAsync(string id)
        {
            return await _collection.Find(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Group group)
        {
            await _collection.InsertOneAsync(group);
        }

        public async Task UpdateAsync(Group group)
        {
            await _collection.ReplaceOneAsync(g => g.Id == group.Id, group);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(g => g.Id == id);
        }
    }
}
