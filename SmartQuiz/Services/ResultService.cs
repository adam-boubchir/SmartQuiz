using MongoDB.Driver;
using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class ResultService : IResultService
    {
        private readonly IMongoCollection<Result> _results;

        public ResultService(IMongoDatabase database)
        {
            _results = database.GetCollection<Result>("Results");
        }

        public async Task<List<Result>> GetResultsByStudentAndTeacherAsync(string studentId, string teacherId)
        {
            var filter = Builders<Result>.Filter.Eq(r => r.StudentId, studentId) &
                         Builders<Result>.Filter.Eq(r => r.TeacherId, teacherId);

            return await _results.Find(filter).ToListAsync();

        }
        public async Task<List<Result>> GetAllResultsByTeacherAsync(string teacherId)
        {
            var filter = Builders<Result>.Filter.Eq(r => r.TeacherId, teacherId);
            return await _results.Find(filter).ToListAsync();
        }

    }
}
