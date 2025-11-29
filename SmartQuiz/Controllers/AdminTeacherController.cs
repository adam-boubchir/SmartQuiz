using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    public class AdminTeacherController : Controller
    {
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;

        public AdminTeacherController(IUserService userService, IGroupService groupService)
        {
            _userService = userService;
            _groupService = groupService;
        }

        // PAGE STATISTIQUES ENSEIGNANTS
        public async Task<IActionResult> Index(string? subject)
        {
            // 1️⃣ Récupérer tous les utilisateurs
            var allUsers = await _userService.GetAllUsers();

            // 2️⃣ Filtrer uniquement les enseignants
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

            // 3️⃣ Récupérer toutes les matières disponibles
            var allSubjects = teachers.SelectMany(t => t.SubjectIds ?? new List<string>())
                                      .Distinct()
                                      .OrderBy(s => s)
                                      .ToList();

            // 4️⃣ Appliquer le filtre matière si fourni
            if (!string.IsNullOrEmpty(subject))
                teachers = teachers.Where(t => t.SubjectIds.Contains(subject)).ToList();

            // 5️⃣ Construire le ViewModel
            var model = new AdminTeachersViewModel
            {
                SelectedSubject = subject,
                Subjects = allSubjects,
                TotalTeachers = teachers.Count,
                TeachersWithGroups = teachers.Count(t => t.GroupIds != null && t.GroupIds.Count > 0),
                Teachers = teachers.Select(t => new TeacherInfoViewModel
                {
                    Id = t.Id.ToString(),
                    FullName = t.Username,
                    Email = t.Email,
                    Subjects = t.SubjectIds,
                    GroupsAssigned = t.GroupIds?.Count ?? 0,
                    TotalHours = t.AssignedHours
                }).ToList()
            };

            return View(model);
        }

        // OPTIONNEL : détails d'un enseignant
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var teacher = await _userService.GetByIdAsync(id);

            if (teacher == null || teacher.Role != "Teacher")
                return NotFound();

            var model = new TeacherInfoViewModel
            {
                Id = teacher.Id.ToString(),
                FullName = teacher.Username,
                Email = teacher.Email,
                Subjects = teacher.SubjectIds,
                GroupsAssigned = teacher.GroupIds?.Count ?? 0,
                TotalHours = teacher.AssignedHours
            };

            return View(model);
        }

        // GET : Modifier un enseignant
// GET : Modifier un enseignant (nom et email seulement)
[HttpGet]
public async Task<IActionResult> EditTeacher(string id)
{
    if (string.IsNullOrEmpty(id))
        return NotFound();

    var teacher = await _userService.GetByIdAsync(id);
    if (teacher == null || teacher.Role != "Teacher")
        return NotFound();

    var model = new EditTeacherViewModel
    {
        Id = teacher.Id.ToString(),
        FullName = teacher.Username,
        Email = teacher.Email
    };

    return View(model);
}

// POST : Modifier un enseignant (nom et email seulement)
[HttpPost]
public async Task<IActionResult> EditTeacher(EditTeacherViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var teacher = await _userService.GetByIdAsync(model.Id);
    if (teacher == null || teacher.Role != "Teacher")
        return NotFound();

    teacher.Username = model.FullName;
    teacher.Email = model.Email;

    // Appeler la méthode du service pour mettre à jour seulement le nom et l'email
    await _userService.UpdateBasicInfoAsync(
        teacher.Id.ToString(),
        teacher.Username,
        teacher.Email
    );

    return RedirectToAction("Index");
}


    // GET : Page assignation groupes et matières
[HttpGet]
public async Task<IActionResult> AssignGroupsSubjects(string id)
{
    if (string.IsNullOrEmpty(id))
        return NotFound();

    var teacher = await _userService.GetByIdAsync(id);
    if (teacher == null || teacher.Role != "Teacher")
        return NotFound();

    var allGroups = await _groupService.GetAllAsync();

    var model = new EditTeacherViewModel
{
    Id = teacher.Id.ToString(),
    FullName = teacher.Username,
    Email = teacher.Email,
    Groups = allGroups,
    SelectedGroupIds = teacher.GroupIds ?? new List<string>(),
    Subjects = teacher.SubjectIds ?? new List<string>(),
    SelectedSubjects = teacher.SubjectIds ?? new List<string>(),
    AssignedHours = teacher.AssignedHours // ajouter ici
};


    return View(model);
}

// POST : Sauvegarder groupes et matières assignés
       [HttpPost]
public async Task<IActionResult> AssignGroupsSubjects(EditTeacherViewModel model)
{
    if (!ModelState.IsValid)
    {
        model.Groups = await _groupService.GetAllAsync();
        return View(model);
    }

    var teacher = await _userService.GetByIdAsync(model.Id);
    if (teacher == null || teacher.Role != "Teacher")
        return NotFound();

    // Ajouter un nouveau groupe si saisi
    if (!string.IsNullOrEmpty(model.NewGroupName))
    {
        var newGroup = new Group { Name = model.NewGroupName };
        await _groupService.CreateAsync(newGroup); // créer dans MongoDB
        model.SelectedGroupIds.Add(newGroup.Id);   // l'ajouter à la sélection
    }

    // Ajouter une nouvelle matière si saisi
    if (!string.IsNullOrEmpty(model.NewSubjectName))
    {
        model.SelectedSubjects.Add(model.NewSubjectName);
    }

    // Mettre à jour l'enseignant
    teacher.GroupIds = model.SelectedGroupIds;
    teacher.SubjectIds = model.SelectedSubjects;
    teacher.AssignedHours = model.AssignedHours;

    await _userService.UpdateTeacherBasicInfoAsync(
        teacher.Id.ToString(),
        teacher.Username,
        teacher.Email,
        teacher.GroupIds,
        teacher.SubjectIds,
        teacher.AssignedHours
    );

    // Message de confirmation
    TempData["SuccessMessage"] = $"Modifications pour {teacher.Username} enregistrées avec succès !";

    // Rediriger vers la page Index
    return RedirectToAction("Index");
}


    }
}
