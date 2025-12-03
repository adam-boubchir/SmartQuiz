using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourProject.Services;
using YourProject.Models;

namespace SmartQuiz.Controllers
{
    public class AdminTeacherController : Controller
    {
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IModuleService _moduleService;
        private readonly IQuizService _quizService;

        public AdminTeacherController(
            IUserService userService,
            IGroupService groupService,
            IModuleService moduleService,
            IQuizService quizService)
        {
            _userService = userService;
            _groupService = groupService;
            _moduleService = moduleService;
            _quizService = quizService;
        }

        // ------------------------------
        // Liste des enseignants
        // ------------------------------
        public async Task<IActionResult> Index()
        {
            var allUsers = await _userService.GetAllUsers();
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

            var model = new AdminTeachersViewModel
            {
                TotalTeachers = teachers.Count,
                Teachers = teachers.Select(t => new TeacherInfoViewModel
                {
                    Id = t.Id.ToString(),
                    FullName = t.Username,
                    Email = t.Email
                }).ToList()
            };

            return View(model);
        }

        // ------------------------------
        // Détails d'un enseignant
        // ------------------------------
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var allUsers = await _userService.GetAllUsers();
            var teacher = allUsers.FirstOrDefault(u => u.Id.ToString() == id && u.Role == "Teacher");
            if (teacher == null) return NotFound();

            var allGroups = await _groupService.GetAllAsync();
            var allModules = await _moduleService.GetAllAsync();
            var allQuizzes = await _quizService.GetAllAsync();

            // Groupes du professeur
            var teacherGroups = allGroups
                .Where(g => g.TeacherId == teacher.Id.ToString())
                .Select(g => new GroupInfo
                {
                    Name = g.Name,
                    Filiere = g.Filiere,
                    Annee = g.Annee,
                    Subject = g.Subject
                })
                .ToList();

            // ------------------------------
            // MODULES AVEC SUBJECTS + GROUPES
            // ------------------------------
            var teacherModules = new List<ModuleInfo>();
            int totalHours = 0;

            foreach (var m in allModules)
            {
                var subjectsForTeacher = new List<ModuleSubjectViewModel>();

                if (m.Subjects != null)
                {
                    foreach (var s in m.Subjects)
                    {
                        if (s.GroupAssignments == null) continue;

                        var relevantAssignments = s.GroupAssignments
                            .Where(ga => ga.TeacherName == teacher.Username)
                            .Select(ga =>
                            {
                                totalHours += ga.Hours;
                                return new ModuleGroupAssignment
                                {
                                    GroupName = ga.GroupName,
                                    TeacherName = ga.TeacherName,
                                    Hours = ga.Hours
                                };
                            })
                            .ToList();

                        if (relevantAssignments.Any())
                        {
                            subjectsForTeacher.Add(new ModuleSubjectViewModel
                            {
                                SubjectName = s.SubjectName,
                                GroupAssignments = relevantAssignments
                            });
                        }
                    }
                }

                if (subjectsForTeacher.Any())
                {
                    teacherModules.Add(new ModuleInfo
                    {
                        Name = m.Name,
                        Filiere = m.Filiere,
                        Annee = int.TryParse(m.Annee, out var a) ? a : 0,
                        Subjects = subjectsForTeacher
                    });
                }
            }

            // ------------------------------
            // QUIZZES DU PROF
            // ------------------------------
            var teacherQuizzes = allQuizzes
                .Where(q => q.CreatedBy == teacher.Id.ToString())
                .Select(q => new QuizInfo
                {
                    Title = q.Title,
                    GroupName = allGroups.FirstOrDefault(g => g.Id == q.GroupId)?.Name,
                    SubjectName = q.SubjectName
                })
                .ToList();

            // ------------------------------
            // FINAL MODEL
            // ------------------------------
            var model = new TeacherFullInfoViewModel
            {
                Id = teacher.Id.ToString(),
                FullName = teacher.Username,
                Email = teacher.Email,
                Groups = teacherGroups,
                Modules = teacherModules,
                Quizzes = teacherQuizzes,
                TotalHours = totalHours
            };

            return View(model);
        }

        // ===================== EDIT INFOS PERSONNELLES =====================
        [HttpGet]
        public async Task<IActionResult> EditTeacher(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var allUsers = await _userService.GetAllUsers();
            var teacher = allUsers.FirstOrDefault(u => u.Id.ToString() == id && u.Role == "Teacher");
            if (teacher == null) return NotFound();

            var model = new TeacherFullInfoViewModel
            {
              Id = teacher.Id.ToString(),
              FullName = teacher.Username,
              Email = teacher.Email
            };

            return View(model);
        }

        [HttpPost]
public async Task<IActionResult> EditTeacher(TeacherFullInfoViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var teacher = await _userService.GetByIdAsync(model.Id);
    if (teacher == null) return NotFound();

    teacher.Username = model.FullName;
    teacher.Email = model.Email;

    await _userService.UpdateAsync(teacher);

    return RedirectToAction("Details", new { id = model.Id });
}

// ===================== EDIT GROUPS =====================
[HttpGet]
public async Task<IActionResult> EditGroups(string id)
{
    if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

    var allGroups = await _groupService.GetAllAsync();
    var teacherGroups = allGroups.Where(g => g.TeacherId == id).ToList();

    return View(teacherGroups); // Créer une vue correspondante
}

// [HttpPost] pour sauvegarder les modifications des groupes
[HttpPost]
public async Task<IActionResult> EditGroups(string id, List<Group> groups)
{
    foreach (var g in groups)
    {
        await _groupService.UpdateAsync(g);
    }

    return RedirectToAction("Details", new { id });
}

// ===================== EDIT MODULES =====================
[HttpGet]
public async Task<IActionResult> EditTeacherModules(string id)
{
    if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

    var allModules = await _moduleService.GetAllAsync();

    // Filtrer uniquement les modules où l'enseignant est assigné
    var teacherModules = allModules
        .Where(m => m.TeacherIds != null && m.TeacherIds.Contains(id))
        .Select(m => new ModuleEditViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Filiere = m.Filiere,
            Annee = int.TryParse(m.Annee, out var annee) ? annee : 0,
            Subjects = (m.Subjects ?? new List<ModuleSubject>())
                .Where(s => !string.IsNullOrEmpty(s.SubjectName)) // filtrer les subjects vides
                .Select(s => new ModuleSubjectEditViewModel
                {
                    SubjectName = s.SubjectName,
                    GroupAssignments = (s.GroupAssignments ?? new List<ModuleGroupAssignment>())
                        .Select(ga => new ModuleGroupAssignmentEditViewModel
                        {
                            GroupName = ga.GroupName,
                            Hours = ga.Hours
                        }).ToList()
                }).ToList()
        })
        .ToList();

    var model = new EditTeacherModulesViewModel
    {
        TeacherId = id,
        Modules = teacherModules
    };

    return View(model);
}


[HttpPost]
public async Task<IActionResult> EditTeacherModules(EditTeacherModulesViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    foreach (var m in model.Modules)
    {
        // Récupérer le module existant
        var module = await _moduleService.GetByIdAsync(m.Id);
        if (module == null) continue;

        module.Name = m.Name;
        module.Filiere = m.Filiere;
        module.Annee = m.Annee.ToString();

        // Mettre à jour les matières et heures
        module.Subjects = m.Subjects.Select(s => new ModuleSubject
        {
            SubjectName = s.SubjectName,
            GroupAssignments = s.GroupAssignments.Select(ga => new ModuleGroupAssignment
            {
                GroupName = ga.GroupName,
                Hours = ga.Hours,
                TeacherName = "" // optionnel si nécessaire
            }).ToList()
        }).ToList();

        // ✅ Fournir ID et objet Module
        await _moduleService.UpdateAsync(module.Id, module);
    }

    return RedirectToAction("Details", new { id = model.TeacherId });
}

// ===================== EDIT QUIZZES =====================
[HttpGet]
public async Task<IActionResult> EditTeacherQuizzes(string id)
{
    if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

    var quizzes = (await _quizService.GetAllAsync())
        .Where(q => q.CreatedBy == id)
        .ToList();

    return View(quizzes);
}

[HttpPost]
public async Task<IActionResult> EditTeacherQuizzes(string id, List<Quiz> quizzes)
{
    foreach (var q in quizzes)
    {
        await _quizService.UpdateAsync(q);
    }

    return RedirectToAction("Details", new { id });
}

    }
}
