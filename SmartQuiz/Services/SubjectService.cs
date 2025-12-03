using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class SubjectService : ISubjectService
{
    private readonly IMongoCollection<Subject> _subjects;

    public SubjectService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _subjects = db.GetCollection<Subject>("Subjects");
    }

    public Task<List<Subject>> GetAllAsync() => _subjects.Find(_ => true).ToListAsync();
    public Task<Subject> GetByIdAsync(string id) => _subjects.Find(s => s.Id == id).FirstOrDefaultAsync();
    public Task CreateAsync(Subject subject) => _subjects.InsertOneAsync(subject);
    public Task UpdateAsync(string id, Subject subject) => _subjects.ReplaceOneAsync(s => s.Id == id, subject);
    public Task DeleteAsync(string id) => _subjects.DeleteOneAsync(s => s.Id == id);
}

}
