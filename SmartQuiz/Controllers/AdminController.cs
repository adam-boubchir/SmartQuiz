using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using Microsoft.AspNetCore.Http;


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
        // Chargement des données
        var students = await _studentService.GetAllAsync();
        var groups = await _groupService.GetByTeacherAsync(""); // all groups si pas filtré
        var allGroups = await _groupService.GetByTeacherAsync(null); // modifie selon ton implementation

        // Statistiques avancées
        int totalGroups = allGroups.Count;
        int studentsWithoutGroup = students.Count(s => s.GroupIds == null || s.GroupIds.Count == 0);
        int average = totalGroups == 0 ? 0 : students.Count / totalGroups;

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
                GroupName = "TODO"
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

            // Affichage de tous les utilisateurs
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

            // Seulement les comptes non approuvés
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
