using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQuiz.Models;
using System;

namespace SmartQuiz.Services
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

        public async Task<List<Quiz>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Quiz>> DebugGetAll()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        public async Task<List<Quiz>> GetForStudentAsync(string groupId)
        {
            var builder = Builders<Quiz>.Filter;

            // Quiz pour tous les groupes OU pour le groupe de l’étudiant
            var filter = builder.Or(
                builder.Eq(q => q.GroupId, null),
                builder.Eq(q => q.GroupId, groupId)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
        public async Task MigrateDatesFromDescription()
        {
            var quizzes = await _collection.Find(q =>
                q.StartDate == null && q.Description.Contains("Ouverture")
            ).ToListAsync();

            foreach (var quiz in quizzes)
            {
                try
                {
                    var parts = quiz.Description.Split("—");
                    var start = parts[0].Replace("Ouverture :", "").Trim();
                    var end = parts[1].Replace("Fermeture :", "").Trim();

                    quiz.StartDate = DateTime.Parse(start);
                    quiz.EndDate = DateTime.Parse(end);

                    await _collection.ReplaceOneAsync(q => q.Id == quiz.Id, quiz);
                }
                catch { /* ignore */ }
            }
        }




    }

}

