using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MongoDB.Bson;

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

        private readonly IOllamaService _ollamaService;
        private readonly IQuizAIService _quizAIService;

        public TeacherController(
            IQuizService quizService,
            ITpService tpService,
            IGroupService groupService,
            IUserService userService,
            IResultService resultService,
            IOllamaService ollamaService,
            IQuizAIService quizAIService)
        {
            _quizService = quizService;
            _tpService = tpService;
            _groupService = groupService;
            _userService = userService;
            _resultService = resultService;

            _ollamaService = ollamaService;
            _quizAIService = quizAIService;
        }

        // -----------------------------------------------------------
        //  DASHBOARD  (/Teacher et /Teacher/Index)
        // -----------------------------------------------------------
        [HttpGet]
        [Route("Teacher")]
        [Route("Teacher/Index")]
        public async Task<IActionResult> Index()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (teacherId == null)
                return RedirectToAction("Login", "Account");

            ViewBag.TotalQuizzes = await _quizService.CountByTeacherAsync(teacherId);
            ViewBag.TotalTps = await _tpService.CountByTeacherAsync(teacherId);
            ViewBag.TotalGroups = await _groupService.CountByTeacherAsync(teacherId);
            ViewBag.TotalStudents = await _groupService.CountStudentsByTeacherAsync(teacherId);

            ViewBag.RecentQuizzes = await _quizService.GetRecentByTeacherAsync(teacherId, 6);
            ViewBag.RecentTps = await _tpService.GetRecentByTeacherAsync(teacherId, 6);

            return View();
        }

        // -----------------------------------------------------------
        //  LISTES
        // -----------------------------------------------------------
       
        [HttpGet]
        [Route("Teacher/Quizzes")]
        public async Task<IActionResult> Quizzes()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine("================================");
            Console.WriteLine("CLAIM teacherId = " + teacherId);

            var quizzes = await _quizService.DebugGetAll();
            Console.WriteLine("TOTAL QUIZZES DB = " + quizzes.Count);


            Console.WriteLine("QUIZZES COUNT = " + quizzes.Count);

            foreach (var q in quizzes)
            {
                Console.WriteLine("QUIZ DB CreatedBy = " + q.CreatedBy);
            }
            Console.WriteLine("================================");

            return View("Quizzes", quizzes);

        }




        public async Task<IActionResult> Tps()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tps = await _tpService.GetByTeacherAsync(teacherId);
            return View(tps);
        }

        public async Task<IActionResult> Groups()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groups = await _groupService.GetByTeacherAsync(teacherId);
            return View(groups);
        }

        // -----------------------------------------------------------
        //  RÉSULTATS
        // -----------------------------------------------------------
        public async Task<IActionResult> StudentsResults()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groups = await _groupService.GetByTeacherAsync(teacherId);

            var students = new List<StudentResultViewModel>();

            foreach (var group in groups)
            {
                foreach (var studentId in group.StudentIds)
                {
                    var student = await _userService.GetByIdAsync(studentId);
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

        public async Task<IActionResult> StudentDetails(string studentId)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
                return RedirectToAction("StudentsResults");

            var results = await _resultService.GetResultsByStudentAndTeacherAsync(studentId, teacherId);
            return View(results);
        }

        public async Task<IActionResult> Results()
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var results = await _resultService.GetAllResultsByTeacherAsync(teacherId);

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

        // -----------------------------------------------------------
        //  PAGE DE CHOIX DU TYPE DE QUIZ
        // -----------------------------------------------------------
        [HttpGet]
        public IActionResult CreateQuiz()
        {
            return View();
        }


        // -----------------------------------------------------------
        //  CRÉATION MANUELLE
        // -----------------------------------------------------------
        public IActionResult CreateQuizManual() => View();

        [HttpPost]
        public async Task<IActionResult> SaveManualQuiz([FromBody] Quiz model)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (teacherId == null)
                return Json(new { success = false, message = "Non authentifié." });

            if (model.Questions == null || model.Questions.Count < 5)
                return Json(new { success = false, message = "Un quiz doit contenir au moins 5 questions." });

            // 🔒 Sécurité & valeurs par défaut
            model.Id = ObjectId.GenerateNewId().ToString();
            model.CreatedBy = teacherId;
            model.CreatedAt = DateTime.UtcNow;
            model.SubjectName ??= "Quiz Manuel";
            model.GroupId = null;

            // 🔧 Nettoyage des questions
            foreach (var q in model.Questions)
            {
                q.Text = q.Text?.Trim();

                if (q.Options == null)
                    q.Options = new List<string>();

                q.Options = q.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();

                // sécurise index correct
                if (q.Options.Count > 0)
                {
                    if (q.CorrectIndex < 0 || q.CorrectIndex >= q.Options.Count)
                        q.CorrectIndex = 0;
                }
                else
                {
                    q.CorrectIndex = 0;
                }
            }

            await _quizService.CreateAsync(model);

            return Json(new { success = true, quizId = model.Id });
        }


        // -----------------------------------------------------------
        //  APERÇU
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> PreviewQuiz(string id)
        {
            var quiz = await _quizService.GetByIdAsync(id);
            return View(quiz);
        }

        // -----------------------------------------------------------
        //  CONFIGURATION
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ConfigureQuiz(string id)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.QuizId = id;
            ViewBag.Groups = await _groupService.GetByTeacherAsync(teacherId);

            return View();
        }

        // -----------------------------------------------------------
        //  PUBLICATION
        // -----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> PublishQuiz([FromBody] PublishQuizRequest req)
        {
            var quiz = await _quizService.GetByIdAsync(req.QuizId);

            if (quiz == null)
                return Json(new { success = false, message = "Quiz introuvable." });

            quiz.GroupId = req.GroupId == "all" ? null : req.GroupId;
            quiz.Description = $"Ouverture : {req.StartDate} — Fermeture : {req.EndDate}";
            quiz.StartDate = DateTime.Parse(req.StartDate);
            quiz.EndDate = DateTime.Parse(req.EndDate);

            // description facultative
            quiz.Description ??= "";


            await _quizService.UpdateAsync(quiz);

            return Json(new { success = true });
        }

        // -----------------------------------------------------------
        //  CRÉATION IA
        // -----------------------------------------------------------
        public IActionResult CreateQuizAI() => View();

        [HttpPost]
        public async Task<IActionResult> GenerateAIQuiz([FromBody] AIRequest request)
        {
            var quiz = new Quiz
            {
                Title = $"Quiz IA - {request.Topic}",
                Questions = await _quizAIService.GenerateQuizAsync(
                    request.Topic, request.Count, request.MultipleChoice)
            };

            return Json(quiz);
        }


        [HttpPost]
        [Route("Teacher/SaveAIQuiz")]
        public async Task<IActionResult> SaveAIQuiz([FromBody] Quiz model)
        {
            Console.WriteLine("🚨 SAVE AI QUIZ APPELÉ");

            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (teacherId == null)
                return Json(new { success = false, message = "Non authentifié." });

            if (model == null || model.Questions == null || !model.Questions.Any())
                return Json(new { success = false, message = "Quiz IA invalide." });

            model.Id = ObjectId.GenerateNewId().ToString();
            model.CreatedBy = teacherId;
            model.CreatedAt = DateTime.UtcNow;
            model.SubjectName = "Quiz IA";
            model.GroupId = null;
            model.Title ??= "Quiz IA";

            await _quizService.CreateAsync(model);

            Console.WriteLine("✅ SAVE AI QUIZ TERMINÉ");

            return Json(new
            {
                success = true,
                quizId = model.Id,
                redirectUrl = Url.Action("ConfigureQuiz", "Teacher", new { id = model.Id })
            });

        }

        // -----------------------------------------------------------
        //  MODIFIER UN QUIZ (GET)
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> EditQuiz(string id)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var quiz = await _quizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound();

            // 🔒 sécurité : seul le prof créateur
            if (quiz.CreatedBy != teacherId)
                return Forbid();

            return View(quiz);
        }
        // -----------------------------------------------------------
        //  MODIFIER UN QUIZ (POST)
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuiz(Quiz model)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (model == null || string.IsNullOrEmpty(model.Id))
                return BadRequest();

            var quiz = await _quizService.GetByIdAsync(model.Id);

            if (quiz == null)
                return NotFound();

            if (quiz.CreatedBy != teacherId)
                return Forbid();

            // ✅ champs autorisés uniquement
            quiz.Title = model.Title;
            quiz.Description = model.Description;

            await _quizService.UpdateAsync(quiz);

            TempData["Success"] = "Quiz modifié avec succès";
            return RedirectToAction("Quizzes");
        }


        // -----------------------------------------------------------
        //  SUPPRIMER UN QUIZ
        // -----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(string id)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(id))
                return Json(new { success = false });

            var quiz = await _quizService.GetByIdAsync(id);

            if (quiz == null)
                return Json(new { success = false, message = "Quiz introuvable" });

            if (quiz.CreatedBy != teacherId)
                return Json(new { success = false, message = "Accès refusé" });

            await _quizService.DeleteAsync(id);

            return Json(new { success = true });
        }










    }

    // DTO Publication
    public class PublishQuizRequest
    {
        public string QuizId { get; set; }
        public string GroupId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    // DTO IA
    public class AIRequest
    {
        public string Topic { get; set; }
        public int Count { get; set; }
        public bool MultipleChoice { get; set; }
    }
}
