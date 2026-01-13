using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Text;


using System.Linq;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;

        private readonly IGroupService _groupService;
        private readonly ModuleService _moduleService;

        public AdminController(IUserService userService, IStudentService studentService, IGroupService groupService, ModuleService moduleService)
        {
            _userService = userService;
            _studentService = studentService;
            _groupService = groupService;
            _moduleService = moduleService;
        }

        public async Task<IActionResult> AllGroups(string? filiere, int? annee)
        {
            // 1) Récupérer tous les groupes
            var allGroups = await _groupService.GetAllAsync(); // List<Group>

            // 2) Construire les listes pour les filtres (sans doublons)
            var filieres = allGroups
            .Where(g => !string.IsNullOrEmpty(g.Filiere))
            .Select(g => g.Filiere)
            .Distinct()
            .OrderBy(f => f)
            .ToList();

            var annees = allGroups
            .Select(g => g.Annee)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

            // 3) Appliquer les filtres
            var filtered = allGroups.AsEnumerable();

            if (!string.IsNullOrEmpty(filiere))
                filtered = filtered.Where(g => g.Filiere == filiere);

            if (annee.HasValue)
                filtered = filtered.Where(g => g.Annee == annee.Value);

            // 4) Mapper vers le ViewModel
            var groupsVm = filtered
                .Select(g => new GroupInfoViewModel
                {
                    Id = g.Id,
                    Nom = g.Name,
                    Filiere = g.Filiere,
                    Annee = g.Annee,
                    Subject = g.Subject,
                    // StudentIds est toujours initialisé = new List<string>()
                    NombreEtudiants = g.StudentIds.Count
                })
                .ToList();

            // 5) Construire le modèle pour la vue
            var model = new AdminGroupsViewModel
            {
                SelectedFiliere = filiere,
                SelectedAnnee = annee,
                Filieres = filieres,
                Annees = annees,
                Groups = groupsVm
            };

            return View(model);
        }

        // GET : Afficher le formulaire
        [HttpGet]
        public async Task<IActionResult> AddGroup()
        {
            var teachers = (await _userService.GetAllUsers()).Where(u => u.Role == "Teacher").ToList();
            var students = (await _userService.GetAllUsers()).Where(u => u.Role == "Student").ToList();

            var model = new AddGroupViewModel
            {
                Teachers = teachers,
                Students = students
            };

            return View(model);
        }

        // POST : Ajouter dans MongoDB
        [HttpPost]
        public async Task<IActionResult> AddGroup(AddGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recharger les listes pour le formulaire
                model.Teachers = (await _userService.GetAllUsers()).Where(u => u.Role == "Teacher").ToList();
                model.Students = (await _userService.GetAllUsers()).Where(u => u.Role == "Student").ToList();
                return View(model);
            }

            var newGroup = new Group
            {
                Name = model.Name,
                Filiere = model.Filiere,
                Annee = model.Annee,
                TeacherId = model.TeacherId,
                StudentIds = model.StudentIds,
                Subject = model.Subject  // maintenant ça compile
            };


            await _groupService.CreateAsync(newGroup);

            return RedirectToAction("AllGroups"); // redirige vers la liste des groupes
        }


         
         public async Task<IActionResult> ExportStudents(string? filiere, int? annee)
{
    // 1️⃣ Récupérer tous les utilisateurs
    var allUsers = await _userService.GetAllUsers();

    // 2️⃣ Filtrer uniquement les étudiants
    var students = allUsers.Where(u => u.Role == "Student").ToList();

    // 3️⃣ Récupérer tous les groupes
    var allGroups = await _groupService.GetAllAsync();
    var groupDict = allGroups.ToDictionary(g => g.Id, g => g);

    // 4️⃣ Appliquer les filtres filière / année (via le groupe)
    if (!string.IsNullOrEmpty(filiere) || annee.HasValue)
    {
        students = students.Where(s =>
        {
            if (string.IsNullOrEmpty(s.GroupId)) return false;
            if (!groupDict.TryGetValue(s.GroupId, out var g)) return false;

            bool ok = true;
            if (!string.IsNullOrEmpty(filiere))
                ok = ok && g.Filiere == filiere;
            if (annee.HasValue)
                ok = ok && g.Annee == annee.Value;

            return ok;
        }).ToList();
    }

    // 5️⃣ Construire le CSV
    var sb = new StringBuilder();
    sb.AppendLine("Nom complet;Email;Filiere;Annee;Groupe");

    foreach (var s in students)
    {
        groupDict.TryGetValue(s.GroupId ?? "", out var g);

        var fullName = s.Username?.Replace(";", ",") ?? "";
        var email = s.Email?.Replace(";", ",") ?? "";
        var fil = g?.Filiere?.Replace(";", ",") ?? "-";
        var an = g?.Annee.ToString() ?? "";
        var gName = g?.Name?.Replace(";", ",") ?? "Non attribué";

        sb.AppendLine($"{fullName};{email};{fil};{an};{gName}");
    }

    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    var fileName = "etudiants";

    if (!string.IsNullOrEmpty(filiere))
        fileName += $"_{filiere}";
    if (annee.HasValue)
        fileName += $"_annee{annee.Value}";

    fileName += ".csv";

    return File(bytes, "text/csv", fileName);
}



        // 🎯 PAGE Students avec statistiques avancées
         public async Task<IActionResult> Students(string? filiere, int? annee)
{
    // 1️⃣ Récupérer tous les utilisateurs
    var allUsers = await _userService.GetAllUsers();

    // 2️⃣ Filtrer uniquement les étudiants
    var students = allUsers.Where(u => u.Role == "Student").ToList();

    // 3️⃣ Récupérer tous les groupes
    var allGroups = await _groupService.GetAllAsync();

    // 4️⃣ Construire les listes pour les filtres (sans doublons)
    var filieres = allGroups
        .Where(g => !string.IsNullOrEmpty(g.Filiere))
        .Select(g => g.Filiere)
        .Distinct()
        .OrderBy(f => f)
        .ToList();

    var annees = allGroups
        .Select(g => g.Annee)
        .Distinct()
        .OrderBy(a => a)
        .ToList();

    // 5️⃣ Appliquer les filtres sur les étudiants selon leur groupe
    if (!string.IsNullOrEmpty(filiere) || annee.HasValue)
    {
        var filteredGroups = allGroups.AsEnumerable();

        if (!string.IsNullOrEmpty(filiere))
            filteredGroups = filteredGroups.Where(g => g.Filiere == filiere);

        if (annee.HasValue)
            filteredGroups = filteredGroups.Where(g => g.Annee == annee.Value);

        var groupIds = filteredGroups.Select(g => g.Id).ToHashSet();
        students = students.Where(s => !string.IsNullOrEmpty(s.GroupId) && groupIds.Contains(s.GroupId)).ToList();
    }

    // 6️⃣ Statistiques
    int totalGroups = allGroups.Count;
    int studentsWithoutGroup = students.Count(s => string.IsNullOrEmpty(s.GroupId));
    int average = totalGroups == 0 ? 0 : students.Count / totalGroups;

    // 7️⃣ Créer un dictionnaire pour lookup rapide des groupes
    var groupDict = allGroups.ToDictionary(g => g.Id, g => g);

    // 8️⃣ Préparer le modèle pour la vue
    var model = new AdminStudentsViewModel
    {
        SelectedFiliere = filiere,
        SelectedAnnee = annee,
        Filieres = filieres,
        Annees = annees,
        TotalStudents = students.Count,
        TotalGroups = totalGroups,
        StudentsWithoutGroup = studentsWithoutGroup,
        AverageStudentsPerGroup = average,
        Students = students.Select(s =>
        {
            groupDict.TryGetValue(s.GroupId ?? "", out var group);

            return new StudentInfoViewModel
            {
                Id = s.Id.ToString(),
                FullName = s.Username,
                Email = s.Email,
                Filiere = group?.Filiere ?? "-",
                Annee = group?.Annee ?? 0,
                GroupName = group?.Name ?? "Non attribué"
            };
        }).ToList(),
        Groups = allGroups.Select(g => new GroupViewModel
        {
            Id = g.Id,
            Name = g.Name
        }).ToList()
    };

    return View(model);
}



// GET : Modifier un étudiant

[HttpGet]
public async Task<IActionResult> EditStudent(string id)
{
    if (string.IsNullOrEmpty(id))
        return NotFound();

    var student = await _userService.GetByIdAsync(id);
    if (student == null || student.Role != "Student")
        return NotFound();

    var allGroups = await _groupService.GetAllAsync();

    // Dropdowns distincts pour filieres et années
    var filieres = allGroups
        .Where(g => !string.IsNullOrEmpty(g.Filiere))
        .Select(g => g.Filiere)
        .Distinct()
        .OrderBy(f => f)
        .ToList();

    var annees = allGroups
        .Select(g => g.Annee)
        .Distinct()
        .OrderBy(a => a)
        .ToList();

    // Récupérer le groupe de l'étudiant
    var studentGroup = allGroups.FirstOrDefault(g => g.Id == student.GroupId);

    // Vérifier que SelectedFiliere et SelectedAnnee existent dans les listes
    string selectedFiliere = studentGroup?.Filiere;
    int? selectedAnnee = studentGroup?.Annee;

    if (!filieres.Contains(selectedFiliere))
        selectedFiliere = null;

    if (selectedAnnee.HasValue && !annees.Contains(selectedAnnee.Value))
        selectedAnnee = null;

    // Préparer le modèle pour la vue
    var model = new EditStudentViewModel
    {
        Id = student.Id.ToString(),
        FullName = student.Username,
        Email = student.Email,
        SelectedGroupId = student.GroupId,
        Groups = allGroups,        // OK car List<Group>
        SelectedFiliere = selectedFiliere,
        Filieres = filieres,
        SelectedAnnee = selectedAnnee,
        Annees = annees
    };

    return View(model);
}

// POST : Modifier un étudiant
[HttpPost]
public async Task<IActionResult> EditStudent(EditStudentViewModel model)
{
    if (!ModelState.IsValid)
    {
        // Recharger les listes pour le formulaire si validation échoue
        var allGroups = await _groupService.GetAllAsync();

        model.Groups = allGroups;

        model.Filieres = allGroups
            .Where(g => !string.IsNullOrEmpty(g.Filiere))
            .Select(g => g.Filiere)
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        model.Annees = allGroups
            .Select(g => g.Annee)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        return View(model);
    }

    // Récupérer l'étudiant depuis la base
    var student = await _userService.GetByIdAsync(model.Id);
    if (student == null || student.Role != "Student")
        return NotFound();

    // Mettre à jour les propriétés
    student.Username = model.FullName;
    student.Email = model.Email;
    student.GroupId = model.SelectedGroupId;

    // Appeler le service pour mettre à jour MongoDB
    await _userService.UpdateStudentBasicInfoAsync(
        student.Id.ToString(),  // Convertir ObjectId en string si nécessaire
        student.Username,
        student.Email,
        student.GroupId
    );

    // Rediriger vers la liste des étudiants
    return RedirectToAction("Students");
}










// GET : Supprimer un étudiant
public async Task<IActionResult> DeleteStudent(string id)
{
    if (string.IsNullOrEmpty(id))
        return NotFound();

   var student = await _userService.GetByIdAsync(id);

    if (student == null || student.Role != "Student")
        return NotFound();

    await _userService.DeleteUser(id);

    return RedirectToAction("Students");
}

        // ✅ PAGE USERS (tous les utilisateurs, sécurisée)
        public async Task<IActionResult> Users()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Account");

            var currentUser = await _userService.GetByEmail(email);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.Role != "Admin")
                return Content("⛔ Accès refusé : réservé aux administrateurs.");

            if (!currentUser.IsApproved)
                return Content("⛔ Votre compte admin est en attente de validation.");

            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // 🎯 PAGE PendingUsers (utilisateurs en attente)
        public async Task<IActionResult> PendingUsers()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Account");

            var currentUser = await _userService.GetByEmail(email);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.Role != "Admin")
                return Content("⛔ Accès refusé : réservé aux administrateurs.");

            if (!currentUser.IsApproved)
                return Content("⛔ Votre compte admin est en attente de validation.");

            var users = await _userService.GetPendingUsers();
            return View(users);
        }

        // ✔ Accepter un utilisateur
        public async Task<IActionResult> Accept(string id)
        {
            await _userService.ActivateUser(id);
            return RedirectToAction("PendingUsers");
        }

        // ✔ Supprimer un utilisateur
        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUser(id);
            return RedirectToAction("PendingUsers");
        }

        // ✅ Dashboard Admin
        public async Task<IActionResult> Dashboard()
        {
            var users = await _userService.GetAllUsers();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                Teachers = users.Count(u => u.Role == "Teacher"),
                Students = users.Count(u => u.Role == "Student"),
                Pending = users.Count(u => !u.IsApproved)
            };

            return View(model);
     
        }

    
    
    }

}

