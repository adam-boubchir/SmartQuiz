using MongoDB.Driver;
using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class StudentAnswerService : IStudentAnswerService
    {
        private readonly IMongoCollection<StudentAnswer> _collection;

        public StudentAnswerService(MongoDbService db)
        {
            _collection = db.GetCollection<StudentAnswer>("StudentAnswers");
        }

        public async Task SaveAnswerAsync(StudentAnswer answer)
        {
            await _collection.InsertOneAsync(answer);
        }

        public async Task<List<StudentAnswer>> GetByStudentAsync(string studentId)
        {
            return await _collection
                .Find(a => a.StudentId == studentId)
                .ToListAsync();
        }
    }
}
