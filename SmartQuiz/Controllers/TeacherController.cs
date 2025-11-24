using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YourProject.Services;
using YourProject.Models;
using System.Security.Claims;


namespace SmartQuiz.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ITpService _tpService;
        private readonly IGroupService _groupService;

        public TeacherController(IQuizService quizService, ITpService tpService, IGroupService groupService)
        {
            _quizService = quizService;
            _tpService = tpService;
            _groupService = groupService;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalQuizzes = await _quizService.CountByTeacherAsync(teacherId);
            ViewBag.TotalTps = await _tpService.CountByTeacherAsync(teacherId);
            ViewBag.TotalGroups = await _groupService.CountByTeacherAsync(teacherId);
            ViewBag.TotalStudents = await _groupService.CountStudentsByTeacherAsync(teacherId);

            ViewBag.RecentQuizzes = await _quizService.GetRecentByTeacherAsync(teacherId, 6);
            ViewBag.RecentTps = await _tpService.GetRecentByTeacherAsync(teacherId, 6);

            return View();
        }

        // GET: /Teacher/Quizzes
        public async Task<IActionResult> Quizzes()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var quizzes = await _quizService.GetByTeacherAsync(teacherId);
            return View(quizzes);
        }

        // GET: /Teacher/Tps
        public async Task<IActionResult> Tps()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tps = await _tpService.GetByTeacherAsync(teacherId);
            return View(tps);
        }

        // GET: /Teacher/Groups
        public async Task<IActionResult> Groups()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groups = await _groupService.GetByTeacherAsync(teacherId);
            return View(groups);
        }

        // Additional endpoints (Create/Edit/Delete) can be added below...
    }
}
