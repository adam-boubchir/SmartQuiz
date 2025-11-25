using System.Threading.Tasks;
using SmartQuiz.Models;
using Microsoft.AspNetCore.Http;

namespace SmartQuiz.Services
{
    public interface IStudentProfileService
    {
        Task<StudentProfileViewModel> GetProfileAsync(string studentId);
        Task UpdateProfileAsync(string studentId, ProfileUpdateModel update);
        Task ChangePasswordAsync(string studentId, string oldPassword, string newPassword);
        Task<string> UploadAvatarAsync(string studentId, IFormFile file); // retourne path sauvegardée
    }
}
