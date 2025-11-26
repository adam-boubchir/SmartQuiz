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
        private readonly ITeacherService _teacherService;
        private readonly IStudentProfileService _studentProfileService;

        public StudentController(
            IQuizService quizService,
            ITpService tpService,
            IStudentAnswerService studentAnswerService,
            ITeacherService teacherService,
            IStudentProfileService studentProfileService,
            IUserService userService)
        {
            _quizService = quizService;
            _tpService = tpService;
            _studentAnswerService = studentAnswerService;
            _userService = userService;
            _teacherService = teacherService;
            _studentProfileService = studentProfileService;
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
        //  PROFIL ÉTUDIANT
        // ======================================
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            var vm = await _studentProfileService.GetProfileAsync(userId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // PROFILE - POST update basic info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileUpdate(ProfileUpdateModel model)
        {
            var userId = GetUserId();
            await _studentProfileService.UpdateProfileAsync(userId, model);
            TempData["Success"] = "Profil mis à jour.";
            return RedirectToAction("Profile");
        }

        // CHANGE PASSWORD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                TempData["Error"] = "La confirmation du mot de passe ne correspond pas.";
                return RedirectToAction("Profile");
            }

            var userId = GetUserId();
            try
            {
                await _studentProfileService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
                TempData["Success"] = "Mot de passe modifié.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Profile");
        }

        // UPLOAD AVATAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null)
            {
                TempData["Error"] = "Fichier manquant.";
                return RedirectToAction("Profile");
            }

            var userId = GetUserId();
            await _studentProfileService.UploadAvatarAsync(userId, avatar);

            TempData["Success"] = "Avatar mis à jour.";
            return RedirectToAction("Profile");
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
