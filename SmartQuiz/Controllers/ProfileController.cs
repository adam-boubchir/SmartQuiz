using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Security.Claims;

namespace SmartQuiz.Controllers
{
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        // -------------------------------------------------------
        // PAGE PROFIL
        // -------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetByIdAsync(userId);
            return View(user);
        }

        // -------------------------------------------------------
        // UPDATE BASIC INFO
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string username, string email)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _userService.UpdateBasicInfoAsync(userId, username, email);

            TempData["SuccessMessage"] = "✔ Profil mis à jour !";
            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // CHANGE PASSWORD
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("Index");
            }

            // Vérification : new != confirm
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "❌ Les mots de passe ne correspondent pas.";
                return RedirectToAction("Index");
            }

            var user = await _userService.GetByIdAsync(userId);

            // Vérifier ancien mot de passe
            if (!_userService.CheckPassword(oldPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "❌ Ancien mot de passe incorrect.";
                return RedirectToAction("Index");
            }

            // Hash & save
            user.PasswordHash = _userService.HashPassword(newPassword);
            await _userService.UpdateAsync(user);

            TempData["SuccessMessage"] = "✔ Mot de passe changé avec succès !";
            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // UPLOAD PROFILE IMAGE
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile profileImage)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (profileImage != null && profileImage.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"{Guid.NewGuid()}_{profileImage.FileName}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                string imageUrl = "/uploads/" + fileName;
                await _userService.UpdateProfileImageAsync(userId, imageUrl);

                // mise à jour de la claim ProfileImage
                var identity = (ClaimsIdentity)User.Identity;

                var existingClaim = identity.FindFirst("ProfileImage");
                if (existingClaim != null)
                    identity.RemoveClaim(existingClaim);

                identity.AddClaim(new Claim("ProfileImage", imageUrl));

                await HttpContext.SignInAsync(
                    new ClaimsPrincipal(identity)
                );
            }

            TempData["SuccessMessage"] = "✔ Photo de profil mise à jour !";
            return RedirectToAction("Index");
        }
    }
}
