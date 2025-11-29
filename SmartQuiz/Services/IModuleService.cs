using SmartQuiz.Models;
public interface IModuleService
{
    Task<List<Module>> GetAllAsync();
    Task<Module> GetByIdAsync(string id);
    Task CreateAsync(Module module);
    Task UpdateAsync(string id, Module module); // <-- ici on ajoute id
    Task DeleteAsync(string id);
}
