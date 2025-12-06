using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using YourProject.Services;

namespace SmartQuiz.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ITpService _tpService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IResultService _resultService;

        public TeacherController(
            IQuizService quizService,
            ITpService tpService,
            IGroupService groupService,
            IUserService userService,
            IResultService resultService)
        {
            _quizService = quizService;
            _tpService = tpService;
            _groupService = groupService;
            _userService = userService;
            _resultService = resultService;
        }

        // --- Dashboard ---
        public async Task<IActionResult> Index()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            ViewBag.TotalQuizzes = await _quizService.CountByTeacherAsync(teacherId);
            ViewBag.TotalTps = await _tpService.CountByTeacherAsync(teacherId);
            ViewBag.TotalGroups = await _groupService.CountByTeacherAsync(teacherId);
            ViewBag.TotalStudents = await _groupService.CountStudentsByTeacherAsync(teacherId);

            ViewBag.RecentQuizzes = await _quizService.GetRecentByTeacherAsync(teacherId, 6);
            ViewBag.RecentTps = await _tpService.GetRecentByTeacherAsync(teacherId, 6);

            return View();
        }

        // --- Quizzes ---
        public async Task<IActionResult> Quizzes()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var quizzes = await _quizService.GetByTeacherAsync(teacherId);
            return View(quizzes);
        }

        // --- TPs ---
        public async Task<IActionResult> Tps()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tps = await _tpService.GetByTeacherAsync(teacherId);
            return View(tps);
        }

        // --- Groups ---
        public async Task<IActionResult> Groups()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groups = await _groupService.GetByTeacherAsync(teacherId);
            return View(groups);
        }

        // --- Liste des étudiants avec leur groupe ---
        public async Task<IActionResult> StudentsResults()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groups = await _groupService.GetByTeacherAsync(teacherId);

            var students = new List<StudentResultViewModel>();

            foreach (var group in groups)
            {
                foreach (var studentId in group.StudentIds)
                {
                    var student = await _userService.GetByIdAsync(studentId); // string
                    if (student != null)
                    {
                        students.Add(new StudentResultViewModel
                        {
                            StudentId = student.Id,
                            StudentName = student.Username,
                            GroupName = group.Name
                        });
                    }
                }
            }

            return View(students);
        }


        // --- Détails des résultats d'un étudiant ---
        public async Task<IActionResult> StudentDetails(string studentId)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
                return RedirectToAction("StudentsResults");

            // studentId est déjà string, filtrage direct
            var results = await _resultService.GetResultsByStudentAndTeacherAsync(studentId, teacherId);

            return View(results);
        }

        public async Task<IActionResult> Results()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Récupérer tous les résultats liés à ce prof
            var results = await _resultService.GetAllResultsByTeacherAsync(teacherId);

            // Calculs statistiques
            var model = new TeacherResultsDashboardViewModel
            {
                TotalEvaluatedStudents = results.Select(r => r.StudentId).Distinct().Count(),
                AverageQuiz = results.Any() ? results.Average(r => r.QuizScore) : 0,
                AverageTest = results.Any() ? results.Average(r => r.TestScore) : 0,
                AverageProgress = results.Any() ? results.Average(r => r.Progression) : 0,

                GroupStats = results
                    .GroupBy(r => r.SubjectName)
                    .Select(g => new GroupStats
                    {
                        Subject = g.Key,
                        AverageQuiz = g.Average(x => x.QuizScore),
                        AverageTest = g.Average(x => x.TestScore),
                        AverageProgress = g.Average(x => x.Progression)
                    })
                    .ToList(),

                TopStudents = results
                    .GroupBy(r => r.StudentId)
                    .Select(g => new StudentScoreSummary
                    {
                        StudentId = g.Key,
                        Average = (g.Average(x => x.QuizScore) + g.Average(x => x.TestScore)) / 2
                    })
                    .OrderByDescending(s => s.Average)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }


    }
}
