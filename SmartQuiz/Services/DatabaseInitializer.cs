using SmartQuiz.Models;
using System;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class DatabaseInitializer
    {
        private readonly IUserService _userService;

        public DatabaseInitializer(IUserService userService)
        {
            _userService = userService;
        }

        public async Task SeedAdminUser()
        {
            // Vérifier si l'admin existe déjà
            var existingAdmin = await _userService.GetByEmail("admin@gmail.com");

            if (existingAdmin != null)
                return;

            // Créer l'Admin par défaut
            var admin = new User
            {
                Email = "admin@gmail.com",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                IsApproved = true
            };

            await _userService.CreateUser(admin);

            Console.WriteLine("✔ Admin auto-créé : admin@gmail.com / admin123");
        }
    }
}
