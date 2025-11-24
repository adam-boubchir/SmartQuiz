using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using YourProject.Services;

namespace SmartQuiz.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ITpService _tpService;
        private readonly IStudentAnswerService _studentAnswerService;
        private readonly IUserService _userService;

        public StudentController(
            IQuizService quizService,
            ITpService tpService,
            IStudentAnswerService studentAnswerService,
            IUserService userService)
        {
            _quizService = quizService;
            _tpService = tpService;
            _studentAnswerService = studentAnswerService;
            _userService = userService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ======================================
        //  PAGE D'ACCUEIL
        // ======================================
        public async Task<IActionResult> Index()
        {
            var studentId = GetUserId();
            var student = await _userService.GetByIdAsync(studentId);

            if (student == null)
                return Unauthorized();

            // Quizzes & TP de tous les profs
            var quizzes = await _quizService.GetByTeachersAsync(student.TeacherIds);
            var tps = await _tpService.GetByTeachersAsync(student.TeacherIds);

            ViewBag.Quizzes = quizzes;
            ViewBag.Tps = tps;

            return View();
        }

        // ======================================
        //  DASHBOARD
        // ======================================
        public async Task<IActionResult> Dashboard()
        {
            var studentId = GetUserId();
            var student = await _userService.GetByIdAsync(studentId);

            if (student == null)
                return Unauthorized();

            var quizzes = await _quizService.GetByTeachersAsync(student.TeacherIds);
            var tps = await _tpService.GetByTeachersAsync(student.TeacherIds);

            ViewBag.QuizzesCount = quizzes.Count;
            ViewBag.TpsCount = tps.Count;

            ViewBag.RecentQuizzes = quizzes.Take(5).ToList();
            ViewBag.RecentTps = tps.Take(5).ToList();

            return View();
        }

        // ======================================
        //  DÉTAIL D’UN QUIZ
        // ======================================
        public async Task<IActionResult> QuizDetail(string id)
        {
            var quiz = await _quizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound();

            return View(quiz);
        }

        // ======================================
        //   SOUMISSION d’un Quiz
        // ======================================
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(StudentAnswer answer)
        {
            answer.StudentId = GetUserId();
            await _studentAnswerService.SaveAnswerAsync(answer);

            return RedirectToAction("MyResults");
        }

        // ======================================
        //  LISTE DE TOUS LES QUIZZES
        // ======================================
        public async Task<IActionResult> Quizzes()
        {
            var student = await _userService.GetByIdAsync(GetUserId());
            var quizzes = await _quizService.GetByTeachersAsync(student.TeacherIds);

            return View(quizzes);
        }

        // ======================================
        //  LISTE DE TOUS LES TP
        // ======================================
        public async Task<IActionResult> Tps()
        {
            var student = await _userService.GetByIdAsync(GetUserId());
            var tps = await _tpService.GetByTeachersAsync(student.TeacherIds);

            return View(tps);
        }

        // ======================================
        //  MES RÉSULTATS
        // ======================================
        public async Task<IActionResult> MyResults()
        {
            var studentId = GetUserId();
            var answers = await _studentAnswerService.GetByStudentAsync(studentId);

            return View(answers);
        }
    }
}
