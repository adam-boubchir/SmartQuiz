using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Security.Claims;

namespace SmartQuiz.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;
        // si tu as d'autres services pour stats : ITeacherService, IStudentService, IGroupService...

        public AdminProfileController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        // -------------------------------------------------------
        // PAGE PROFIL ADMIN
        // -------------------------------------------------------
        public async Task<IActionResult> Profile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null) return RedirectToAction("Login", "Account");

    var user = await _userService.GetByIdAsync(userId);
    if (user == null) return RedirectToAction("Login", "Account");

    var model = new AdminProfileViewModel
    {
        Id = user.Id.ToString(),
        Username = user.Username,
        Email = user.Email,
        AvatarPath = user.ProfileImage, // <-- toujours depuis la base
    };

    return View(model);
}


        // -------------------------------------------------------
        // UPDATE BASIC INFO (ADMIN)
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ProfileUpdate(string username, string email)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("Profile");
            }

            await _userService.UpdateBasicInfoAsync(userId, username, email);

            TempData["SuccessMessage"] = "✔ Profil administrateur mis à jour !";
            return RedirectToAction("Profile");
        }

        // -------------------------------------------------------
        // CHANGE PASSWORD (ADMIN)
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmNewPassword)
            {
                TempData["ErrorMessage"] = "❌ Les mots de passe ne correspondent pas.";
                return RedirectToAction("Profile");
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("Profile");
            }

            if (!_userService.CheckPassword(oldPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "❌ Ancien mot de passe incorrect.";
                return RedirectToAction("Profile");
            }

            user.PasswordHash = _userService.HashPassword(newPassword);
            await _userService.UpdateAsync(user);

            TempData["SuccessMessage"] = "✔ Mot de passe admin changé avec succès !";
            return RedirectToAction("Profile");
        }

        // -------------------------------------------------------
        // UPLOAD PROFILE IMAGE (ADMIN)
        // -------------------------------------------------------
        [HttpPost]
public async Task<IActionResult> UploadAvatar(IFormFile avatar)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null) return RedirectToAction("Profile");

    if (avatar != null && avatar.Length > 0)
    {
        string folderPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = $"{Guid.NewGuid()}_{avatar.FileName}";
        string filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        string imageUrl = "/uploads/" + fileName;

        // 🔹 Stocker l'image dans la base
        await _userService.UpdateProfileImageAsync(userId, imageUrl);

        TempData["SuccessMessage"] = "✔ Photo de profil admin mise à jour !";
    }

    return RedirectToAction("Profile");
}

    }
}
