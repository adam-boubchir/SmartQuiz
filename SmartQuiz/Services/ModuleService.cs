using MongoDB.Driver;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IMongoCollection<Module> _modules;

        public ModuleService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _modules = db.GetCollection<Module>("Modules");
        }

        public async Task<List<Module>> GetAllAsync()
        {
            return await _modules.Find(_ => true).ToListAsync();
        }

        public async Task<Module> GetByIdAsync(string id)
        {
            return await _modules.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Module module)
        {
            await _modules.InsertOneAsync(module);
        }

        public async Task UpdateAsync(string id, Module module)
        {
            await _modules.ReplaceOneAsync(m => m.Id == id, module);
        }

        public async Task DeleteAsync(string id)
        {
            await _modules.DeleteOneAsync(m => m.Id == id);
        }
    }
}
