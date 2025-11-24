using MongoDB.Driver;
using SmartQuiz.Models;
using SmartQuuiz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourProject.Services;

namespace SmartQuiz.Services
{
    public class StudentService : IStudentService
    {
        private readonly IMongoCollection<Student> _collection;
        private readonly IGroupService _groupService;

        public StudentService(IMongoDatabase database, IGroupService groupService)
        {
            _collection = database.GetCollection<Student>("Students");
            _groupService = groupService;
        }

        public async Task<Student> GetByIdAsync(string id)
        {
            return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Student> GetByEmailAsync(string email)
        {
            return await _collection.Find(s => s.Email == email).FirstOrDefaultAsync();
        }

        public async Task<List<Student>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Student>> GetByGroupAsync(string groupId)
        {
            var filter = Builders<Student>.Filter.AnyEq(s => s.GroupIds, groupId);

            return await _collection
                .Find(filter)
                .SortByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Student student)
        {
            student.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(student);
        }

        public async Task UpdateAsync(Student student)
        {
            await _collection.ReplaceOneAsync(s => s.Id == student.Id, student);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(s => s.Id == id);
        }

        // ==============================
        //   MÉTHODES POUR LE TEACHER
        // ==============================

        public async Task<long> CountByTeacherAsync(string teacherId)
        {
            // Count students that have this teacher
            var filter = Builders<Student>.Filter.AnyEq(s => s.TeacherIds, teacherId);

            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Student>> GetRecentByTeacherAsync(string teacherId, int limit = 5)
        {
            var filter = Builders<Student>.Filter.AnyEq(s => s.TeacherIds, teacherId);

            return await _collection
                .Find(filter)
                .SortByDescending(s => s.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
    }
}
