using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using Microsoft.AspNetCore.Http;


namespace SmartQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;

        public AdminController(UserService userService)
        {
            _userService = userService;
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

        public async Task<IActionResult> Teachers()
{
    var email = HttpContext.Session.GetString("UserEmail");
    if (email == null) return RedirectToAction("Login", "Account");

    var currentUser = await _userService.GetByEmail(email);
    if (currentUser == null || currentUser.Role != "Admin")
        return Content("⛔ Accès refusé");
    if (!currentUser.IsApproved)
        return Content("⛔ Votre compte admin est en attente de validation.");

    var allUsers = await _userService.GetAllUsers();
    var teachersList = allUsers.Where(u => u.Role == "Teacher").ToList();

    var model = new SmartQuiz.Models.TeachersListViewModel();

    foreach (var teacher in teachersList)
    {
        model.Teachers.Add(new SmartQuiz.Models.TeacherInfoViewModel
        {
            Id = teacher.Id.ToString(),
            Username = teacher.Username,
            Email = teacher.Email,
            GroupsCount = 0,
            StudentsCount = 0,
            QuizCount = 0,
            TpCount = 0
        });
    }

    return View(model);
}

    }
}
