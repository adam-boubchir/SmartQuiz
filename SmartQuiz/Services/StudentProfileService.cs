using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartQuiz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace SmartQuiz.Services
{
    public class StudentProfileService : IStudentProfileService
    {
        private readonly IUserService _userService;
        private readonly ITeacherService _teacherService; // ou ITeacherService équivalent
        private readonly IQuizService _quizService;
        private readonly ITpService _tpService;
        private readonly IWebHostEnvironment _env;
        private readonly IMongoDatabase _database;

        public StudentProfileService(
            IUserService userService,
            ITeacherService teacherService,
            IQuizService quizService,
            ITpService tpService,
            IWebHostEnvironment env,
            IMongoDatabase database)
        {
            _userService = userService;
            _teacherService = teacherService;
            _quizService = quizService;
            _tpService = tpService;
            _env = env;
            _database = database;
        }

        public async Task<StudentProfileViewModel> GetProfileAsync(string studentId)
        {
            var user = await _userService.GetByIdAsync(studentId);
            if (user == null) return null;

            // teachers: suppose user has TeacherIds list (if not, adjust accordingly)
            var teacherDtos = new List<TeacherDto>();
            var teacherIds = new List<string>();

            // If your User model stores teacher ids as List<string> TeacherIds
            var userObj = user; // dynamic reference
            try
            {
                // try to read TeacherIds property via reflection (safe)
                var prop = userObj.GetType().GetProperty("TeacherIds");
                if (prop != null)
                {
                    var val = prop.GetValue(userObj) as IEnumerable<string>;
                    if (val != null) teacherIds = val.ToList();
                }
            }
            catch { /* ignore */ }

            if (teacherIds.Any() && _teacherService != null)
            {
                foreach (var tId in teacherIds)
                {
                    var t = await _teacherService.GetByIdAsync(tId);
                    if (t != null)
                    {
                        teacherDtos.Add(new TeacherDto { Id = t.Id.ToString(), Name = t.Username ?? t.Email, Email = t.Email });
                    }
                }
            }

            // stats: count quizzes/tps assigned to this student via teachers
            var quizzes = (teacherIds.Any() && _quizService != null) ? await _quizService.GetByTeachersAsync(teacherIds) : new List<SmartQuiz.Models.Quiz>();
            var tps = (teacherIds.Any() && _tpService != null) ? await _tpService.GetByTeachersAsync(teacherIds) : new List<SmartQuiz.Models.Tp>();

            var vm = new StudentProfileViewModel
            {
                Id = studentId,
                Email = user.Email,
                Username = user.Username ?? "",
                AvatarPath = (user.GetType().GetProperty("AvatarPath") != null) ? (string)user.GetType().GetProperty("AvatarPath").GetValue(user) : null,
                GroupId = (user.GetType().GetProperty("GroupId") != null) ? (string)user.GetType().GetProperty("GroupId").GetValue(user) : null,
                Teachers = teacherDtos,
                TotalQuizzes = quizzes?.Count ?? 0,
                TotalTps = tps?.Count ?? 0,
                // Completed counts can be derived from StudentAnswer service, implement if available
                CompletedQuizzes = 0,
                CompletedTps = 0
            };

            return vm;
        }

        public async Task UpdateProfileAsync(string studentId, ProfileUpdateModel update)
        {
            var user = await _userService.GetByIdAsync(studentId);
            if (user == null) throw new Exception("Utilisateur introuvable");

            user.Username = update.Username ?? user.Username;
            user.Email = update.Email ?? user.Email;

            await _userService.UpdateAsync(user);
        }

        public async Task ChangePasswordAsync(string studentId, string oldPassword, string newPassword)
        {
            var user = await _userService.GetByIdAsync(studentId);
            if (user == null) throw new Exception("Utilisateur introuvable");

            // IUserService must expose CheckPassword and ChangePassword/SetPassword
            if (!_userService.CheckPassword(oldPassword, user.PasswordHash))
                throw new Exception("Mot de passe actuel incorrect");

            // update password
            var hashed = _userService.HashPassword(newPassword);
            user.PasswordHash = hashed;
            await _userService.UpdateAsync(user);
        }

        public async Task<string> UploadAvatarAsync(string studentId, IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{studentId}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // store relative path (to serve via /uploads/avatars/...)
            var relativePath = $"/uploads/avatars/{fileName}";

            var user = await _userService.GetByIdAsync(studentId);
            if (user != null)
            {
                // ensure property AvatarPath exists or use dynamic update
                var prop = user.GetType().GetProperty("AvatarPath");
                if (prop != null) prop.SetValue(user, relativePath);
                else
                {
                    // if User model doesn't have AvatarPath, consider adding it
                }
                await _userService.UpdateAsync(user);
            }

            return relativePath;
        }
    }
}
