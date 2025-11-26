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

        // ⚡ Affiche le profil
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetByIdAsync(userId);

            return View(user);
        }

        // ⚡ Mise à jour des informations (sans image)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string username, string email)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _userService.UpdateBasicInfoAsync(userId, username, email);

            TempData["Success"] = "Profil mis à jour !";
            return RedirectToAction("Index");
        }

        // ⚡ Upload de la photo de profil
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

                // Mise à jour de la claim ProfileImage
                var identity = (ClaimsIdentity)User.Identity;
                identity.RemoveClaim(identity.FindFirst("ProfileImage"));
                identity.AddClaim(new Claim("ProfileImage", imageUrl));

                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            }

            TempData["Success"] = "Photo de profil mise à jour !";
            return RedirectToAction("Index");
        }
    }
}
