using MongoDB.Driver;
using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IMongoCollection<Module> _modules;

        public ModuleService(IMongoDatabase db)
        {
            _modules = db.GetCollection<Module>("Modules");
        }

        public Task<List<Module>> GetAllAsync() => _modules.Find(_ => true).ToListAsync();

        public Task<Module> GetByIdAsync(string id) => _modules.Find(m => m.Id == id).FirstOrDefaultAsync();

        public Task CreateAsync(Module module) => _modules.InsertOneAsync(module);

        public Task UpdateAsync(string id, Module module) => _modules.ReplaceOneAsync(m => m.Id == id, module);

        public Task DeleteAsync(string id) => _modules.DeleteOneAsync(m => m.Id == id);
    }
}
