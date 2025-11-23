using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;
using System;

namespace YourProject.Services
{
    public class TpService : ITpService
    {
        private readonly IMongoCollection<Tp> _collection;

        public TpService(IMongoDatabase database)
        {
            _collection = database.GetCollection<Tp>("Tps");
        }

        public async Task<long> CountByTeacherAsync(string teacherId)
        {
            var filter = Builders<Tp>.Filter.Eq(t => t.teacherId, teacherId);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Tp>> GetRecentByTeacherAsync(string teacherId, int limit = 5)
        {
            var filter = Builders<Tp>.Filter.Eq(t => t.teacherId, teacherId);
            var sort = Builders<Tp>.Sort.Descending(t => t.CreatedAt);
            return await _collection.Find(filter).Sort(sort).Limit(limit).ToListAsync();
        }

        public async Task<List<Tp>> GetByTeacherAsync(string teacherId)
        {
            var filter = Builders<Tp>.Filter.Eq(t => t.teacherId, teacherId);
            return await _collection.Find(filter).SortByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<Tp> GetByIdAsync(string id)
        {
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Tp tp)
        {
            tp.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(tp);
        }

        public async Task UpdateAsync(Tp tp)
        {
            await _collection.ReplaceOneAsync(t => t.Id == tp.Id, tp);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(t => t.Id == id);
        }
    }
}
