using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using Microsoft.AspNetCore.Http;

namespace SmartQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService; // ✅ Utiliser l'interface

        public AdminController(IUserService userService) // ✅ Injection correcte
        {
            _userService = userService;
        }

        // ✅ PAGE USERS (tous les utilisateurs)
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

        // 🎯 PAGE PendingUsers (comptes non approuvés)
        public async Task<IActionResult> PendingUsers()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Account");

            var currentUser = await _userService.GetByEmail(email);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (currentUser.Role != "Admin")
                return Content("⛔ Accès refusé.");

            if (!currentUser.IsApproved)
                return Content("⛔ Votre compte admin doit être validé.");

            var users = await _userService.GetPendingUsers();
            return View(users);
        }

        // ✔ Valider un utilisateur
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

        // 📊 Dashboard Admin
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
