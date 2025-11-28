using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;

using System.Linq;
using System.Threading.Tasks;
using YourProject.Models;

namespace SmartQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;

        private readonly IGroupService _groupService;

        public AdminController(IUserService userService, IStudentService studentService, IGroupService groupService)
        {
            _userService = userService;
            _studentService = studentService;
            _groupService = groupService;
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


        // 🎯 PAGE Students avec statistiques avancées
        public async Task<IActionResult> Students()
        {
            // 1️⃣ Récupérer tous les utilisateurs
            var allUsers = await _userService.GetAllUsers();

            // 2️⃣ Filtrer uniquement les étudiants
            var students = allUsers.Where(u => u.Role == "Student").ToList();

            // 3️⃣ Récupérer tous les groupes
            var allGroups = await _groupService.GetAllAsync();

            // 4️⃣ Statistiques
            int totalGroups = allGroups.Count;
            int studentsWithoutGroup = students.Count(s => string.IsNullOrEmpty(s.GroupId));
            int average = totalGroups == 0 ? 0 : students.Count / totalGroups;

            // 5️⃣ Préparer le modèle pour la vue
            var model = new AdminStudentsViewModel
            {
                TotalStudents = students.Count,
                TotalGroups = totalGroups,
                StudentsWithoutGroup = studentsWithoutGroup,
                AverageStudentsPerGroup = average,
                Students = students.Select(s => new StudentInfoViewModel
                {
                    Id = s.Id.ToString(),
                    FullName = s.Username,
                    Email = s.Email,
                    GroupName = string.IsNullOrEmpty(s.GroupId)
                        ? "Non attribué"
                        : allGroups.FirstOrDefault(g => g.Id == s.GroupId)?.Name ?? "Non attribué"
                }).ToList(),
                Groups = allGroups.Select(g => new GroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList()
            };

            return View(model);
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

