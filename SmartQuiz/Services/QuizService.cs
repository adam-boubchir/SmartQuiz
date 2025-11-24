using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourProject.Models;
using System;

namespace YourProject.Services
{
    public class QuizService : IQuizService
    {
        private readonly IMongoCollection<Quiz> _collection;

        public QuizService(IMongoDatabase database)
        {
            _collection = database.GetCollection<Quiz>("Quizzes");
        }

        public async Task<long> CountByTeacherAsync(string teacherId)
        {
            var filter = Builders<Quiz>.Filter.Eq(q => q.CreatedBy, teacherId);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Quiz>> GetRecentByTeacherAsync(string teacherId, int limit = 5)
        {
            var filter = Builders<Quiz>.Filter.Eq(q => q.CreatedBy, teacherId);
            var sort = Builders<Quiz>.Sort.Descending(q => q.CreatedAt);
            return await _collection.Find(filter).Sort(sort).Limit(limit).ToListAsync();
        }

        public async Task<List<Quiz>> GetByTeacherAsync(string teacherId)
        {
            var filter = Builders<Quiz>.Filter.Eq(q => q.CreatedBy, teacherId);
            return await _collection.Find(filter).SortByDescending(q => q.CreatedAt).ToListAsync();
        }

        // ⭐⭐ NEW
        public async Task<List<Quiz>> GetByTeachersAsync(List<string> teacherIds)
        {
            var filter = Builders<Quiz>.Filter.In(q => q.CreatedBy, teacherIds);
            return await _collection.Find(filter).SortByDescending(q => q.CreatedAt).ToListAsync();
        }

        public async Task<Quiz> GetByIdAsync(string id)
        {
            return await _collection.Find(q => q.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Quiz>> GetByGroupAsync(string groupId)
        {
            var filter = Builders<Quiz>.Filter.Eq(q => q.GroupId, groupId);
            return await _collection.Find(filter).SortByDescending(q => q.CreatedAt).ToListAsync();
        }

        public async Task CreateAsync(Quiz quiz)
        {
            quiz.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(quiz);
        }

        public async Task UpdateAsync(Quiz quiz)
        {
            await _collection.ReplaceOneAsync(q => q.Id == quiz.Id, quiz);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(q => q.Id == id);
        }
    }

}

