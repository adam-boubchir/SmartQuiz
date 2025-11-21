using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Security.Claims;

namespace SmartQuiz.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userService.GetByEmail(email);
            if (user == null)
            {
                ViewBag.Error = "Email incorrect";
                return View();
            }

            var hash = _userService.HashPassword(password);
            if (hash != user.PasswordHash)
            {
                ViewBag.Error = "Mot de passe incorrect";
                return View();
            }

            // Auth cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string role)
        {
            var exists = await _userService.GetByEmail(email);
            if (exists != null)
            {
                ViewBag.Error = "Cet email est déjà utilisé";
                return View();
            }

            var user = new User
            {
                Email = email,
                PasswordHash = _userService.HashPassword(password),
                Role = role
            };

            await _userService.CreateUser(user);

            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
