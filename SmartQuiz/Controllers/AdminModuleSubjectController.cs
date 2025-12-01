using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    public class AdminModuleSubjectController : Controller
    {
        private readonly IModuleService _moduleService;
        private readonly ISubjectService _subjectService;
        private readonly IUserService _userService;


        public AdminModuleSubjectController(IModuleService moduleService, ISubjectService subjectService, IUserService userService)
        {
            _moduleService = moduleService;
            _subjectService = subjectService;
            _userService = userService;
        }

        // -------------------- LISTE MODULES & MATIÈRES --------------------
        public async Task<IActionResult> ModulesAndSubjects()
{
    var modules = await _moduleService.GetAllAsync();
    var viewModel = new ModulesViewModel();
    

    foreach (var m in modules)
    {
        m.Subjects ??= new List<ModuleSubject>();

        int totalModuleHours = m.Subjects.SelectMany(s => s.GroupAssignments ?? new List<ModuleGroupAssignment>()).Sum(g => g.Hours);

        foreach (var subj in m.Subjects)
        {
            subj.GroupAssignments ??= new List<ModuleGroupAssignment>();
            int totalSubjectHours = subj.TotalHours;

            if (!subj.GroupAssignments.Any())
            {
                viewModel.Modules.Add(new ModuleRowViewModel
                {
                    ModuleId = m.Id,
                    ModuleName = m.Name,
                    Filiere = m.Filiere,
                    Annee = m.Annee,
                    SubjectName = subj.SubjectName,
                    GroupName = "-",
                    TeacherName = "-",
                    GroupHours = 0,
                    TotalSubjectHours = totalSubjectHours,
                    TotalModuleHours = totalModuleHours
                });
            }
            else
            {
                foreach (var grp in subj.GroupAssignments)
                {
                    viewModel.Modules.Add(new ModuleRowViewModel
                    {
                        ModuleId = m.Id,
                        ModuleName = m.Name,
                        Filiere = m.Filiere,
                        Annee = m.Annee,
                        SubjectName = subj.SubjectName,
                        GroupName = grp.GroupName,
                        TeacherName = grp.TeacherName,
                        GroupHours = grp.Hours,
                        TotalSubjectHours = totalSubjectHours,
                        TotalModuleHours = totalModuleHours
                    });
                }
            }
        }
    }

    return View(viewModel);
}



        // GET : Création module
        [HttpGet]
        public IActionResult CreateModule()
        {
           return View();
        }


        // POST : Création module
         [HttpPost]
public async Task<IActionResult> CreateModule(Module module)
{
    if (module == null || string.IsNullOrEmpty(module.Name))
    {
        ModelState.AddModelError("", "Le nom du module est requis.");
        return View(module);
    }

    module.Annee = string.IsNullOrEmpty(module.Annee) ? "2025" : module.Annee;

    // ----------------------------
    // Créer la structure imbriquée à partir des champs du formulaire
    // ----------------------------
    if ((module.Subjects == null || !module.Subjects.Any()) &&
        !string.IsNullOrEmpty(Request.Form["SubjectName"]))
    {
        module.Subjects = new List<ModuleSubject>();

        int subjectCount = Request.Form["SubjectName"].Count;

        for (int i = 0; i < subjectCount; i++)
        {
            var subjectName = Request.Form[$"Subjects[{i}].SubjectName"].ToString();
            var groupCount = Request.Form[$"Subjects[{i}].GroupAssignments.Count"].Count;

            var groupAssignments = new List<ModuleGroupAssignment>();

            for (int j = 0; j < groupCount; j++)
            {
                string groupName = Request.Form[$"Subjects[{i}].GroupAssignments[{j}].GroupName"];
                string teacherName = Request.Form[$"Subjects[{i}].GroupAssignments[{j}].TeacherName"];
                int hours = int.TryParse(Request.Form[$"Subjects[{i}].GroupAssignments[{j}].Hours"], out var h) ? h : 0;

                groupAssignments.Add(new ModuleGroupAssignment
                {
                    GroupName = groupName,
                    TeacherName = teacherName,
                    Hours = hours
                });
            }

            module.Subjects.Add(new ModuleSubject
            {
                SubjectName = subjectName,
                GroupAssignments = groupAssignments
            });
        }
    }

    // ----------------------------
    // Vérifier si le module existe déjà
    // ----------------------------
    var existingModule = (await _moduleService.GetAllAsync()).FirstOrDefault(m => m.Name == module.Name);

    if (existingModule == null)
    {
        await _moduleService.CreateAsync(module);
    }
    else
    {
        // Fusionner matières et groupes
        foreach (var newSubj in module.Subjects)
        {
            var existingSubj = existingModule.Subjects.FirstOrDefault(s => s.SubjectName == newSubj.SubjectName);

            if (existingSubj == null)
            {
                existingModule.Subjects.Add(newSubj);
            }
            else
            {
                existingSubj.GroupAssignments.AddRange(newSubj.GroupAssignments);
            }
        }

        await _moduleService.UpdateAsync(existingModule.Id, existingModule);
    }

    return RedirectToAction("ModulesAndSubjects");
}


        // GET : Modifier module
[HttpGet]
public async Task<IActionResult> EditModule(string id)
{
    if (string.IsNullOrEmpty(id))
        return RedirectToAction("ModulesAndSubjects");

    var module = await _moduleService.GetByIdAsync(id);
    if (module == null)
        return NotFound();

    // La vue pourra boucler sur module.Subjects et leurs GroupAssignments
    return View(module);
}

// POST : Modifier module
[HttpPost]
public async Task<IActionResult> EditModule(Module module)
{
    if (module == null || string.IsNullOrEmpty(module.Id))
        return NotFound();

    // Récupérer le module existant depuis MongoDB
    var existingModule = await _moduleService.GetByIdAsync(module.Id);
    if (existingModule == null)
        return NotFound();

    // 1️⃣ Mettre à jour les champs simples
    existingModule.Name = module.Name;
    existingModule.Filiere = module.Filiere;
    existingModule.Annee = module.Annee;

    // 2️⃣ Mettre à jour les matières et leurs groupes
    if (module.Subjects != null && module.Subjects.Any())
    {
        foreach (var newSubj in module.Subjects)
        {
            // Vérifier si la matière existe déjà dans le module
            var existingSubj = existingModule.Subjects
                .FirstOrDefault(s => s.SubjectName == newSubj.SubjectName);

            if (existingSubj == null)
            {
                // Matière nouvelle → on l'ajoute
                existingModule.Subjects.Add(newSubj);
            }
            else
            {
                // Matière existante → on met à jour les groupes
                if (newSubj.GroupAssignments != null)
                {
                    existingSubj.GroupAssignments = newSubj.GroupAssignments;
                }
                else
                {
                    existingSubj.GroupAssignments = new List<ModuleGroupAssignment>();
                }
            }
        }
    }

    // 3️⃣ Sauvegarder dans MongoDB
    await _moduleService.UpdateAsync(existingModule.Id, existingModule);

    return RedirectToAction("ModulesAndSubjects");
}


        // Supprimer module
       public async Task<IActionResult> DeleteModuleRow(string moduleId, string subjectName, string groupName)
{
    var module = await _moduleService.GetByIdAsync(moduleId);
    if (module == null) return NotFound();

    var subject = module.Subjects.FirstOrDefault(s => s.SubjectName == subjectName);
    if (subject != null)
    {
        var group = subject.GroupAssignments.FirstOrDefault(g => g.GroupName == groupName);
        if (group != null)
        {
            subject.GroupAssignments.Remove(group);

            // Si la matière n'a plus de groupe, tu peux aussi la supprimer
            if (!subject.GroupAssignments.Any())
                module.Subjects.Remove(subject);
        }
    }

    await _moduleService.UpdateAsync(module.Id, module);
    return RedirectToAction("ModulesAndSubjects");
}

        // -------------------- MATIÈRES --------------------
        // GET : Formulaire création matière
        public IActionResult CreateSubject()
        {
            return View();
        }

        // POST : Création matière
        [HttpPost]
        public async Task<IActionResult> CreateSubject(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("", "Nom requis");
                return View();
            }

            await _subjectService.CreateAsync(new Subject { Name = name });
            return RedirectToAction("ModulesAndSubjects");
        }

        // GET : Modifier matière
        public async Task<IActionResult> EditSubject(string id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        // POST : Modifier matière
        [HttpPost]
        public async Task<IActionResult> EditSubject(Subject subject)
        {
            if (subject == null) return NotFound();
            await _subjectService.UpdateAsync(subject.Id, subject);
            return RedirectToAction("ModulesAndSubjects");
        }

        // Supprimer matière
        public async Task<IActionResult> DeleteSubject(string id)
        {
            await _subjectService.DeleteAsync(id);
            return RedirectToAction("ModulesAndSubjects");
        }

        // -------------------- AUTOCOMPLETE ENSEIGNANTS --------------------
        [HttpGet]
public async Task<JsonResult> SearchTeachers(string query)
{
    if (string.IsNullOrWhiteSpace(query))
        return Json(new List<string>());

    var teachers = await _userService.GetAllUsers();

    var filtered = teachers
        .Where(t => t != null 
                    && t.Role != null && t.Role == "Teacher"
                    && !string.IsNullOrEmpty(t.Username) 
                    && t.Username.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        .Select(t => t.Username)
        .ToList();

    return Json(filtered);
}



        
    }
}
