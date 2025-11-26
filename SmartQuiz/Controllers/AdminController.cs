using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;
        private readonly StudentService _studentService;
        private readonly GroupService _groupService;

        public AdminController(UserService userService, StudentService studentService, GroupService groupService)
        {
            _userService = userService;
            _studentService = studentService;
            _groupService = groupService;
        }

        // 🎯 PAGE Students avec statistiques avancées
        public async Task<IActionResult> Students()
        {
            // Récupération des étudiants
            var students = await _studentService.GetAllAsync();

            // Récupération des groupes (null = tous les groupes)
            var allGroups = await _groupService.GetByTeacherAsync(null);

            // Statistiques
            int totalGroups = allGroups.Count;
            int studentsWithoutGroup = students.Count(s => s.GroupIds == null || s.GroupIds.Count == 0);
            int average = totalGroups == 0 ? 0 : students.Count / totalGroups;

            // Préparer le modèle pour la vue
            var model = new AdminStudentsViewModel
            {
                TotalStudents = students.Count,
                TotalGroups = totalGroups,
                StudentsWithoutGroup = studentsWithoutGroup,
                AverageStudentsPerGroup = average,
                Students = students.Select(s => new StudentInfoViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    // Optionnel : récupérer le nom du groupe principal
                    GroupName = s.GroupIds != null && s.GroupIds.Count > 0
                        ? allGroups.FirstOrDefault(g => g.Id == s.GroupIds[0])?.Name ?? "Non attribué"
                        : "Non attribué"
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
