using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;

namespace SmartQuiz.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // PAGE D’ACCUEIL
        public IActionResult Index()
        {
            return View();
        }

        // ======================
        // 🔵 PAGES PUBLIQUES
        // ======================

        public IActionResult Pourquoi()
        {
            return View();
        }

        public IActionResult Temoignages()
        {
            return View();
        }

        public IActionResult CommentCaMarche()
        {
            return View();
        }

        public IActionResult Galerie()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        public IActionResult Objectifs()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Technologies()
        {
            return View();
        }

        public IActionResult CTA()
        {
            return View();
        }

        // ======================
        // AUTRES
        // ======================

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
