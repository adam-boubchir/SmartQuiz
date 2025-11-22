using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class DatabaseInitializer
    {
        private readonly UserService _userService;

        public DatabaseInitializer(UserService userService)
        {
            _userService = userService;
        }

        public async Task SeedAdminUser()
        {
            // Vérifier si l'admin existe déjà
            var admin = await _userService.GetByEmail("admin@gmail.com");

            if (admin != null)
                return;

            // Créer l'Admin par défaut
            var newAdmin = new User
            {
                Email = "admin@gmail.com",
                Username = "admin",
                PasswordHash = "admin123",  // sera hashé automatiquement
                Role = "Admin"
            };

            await _userService.CreateUser(newAdmin);

            Console.WriteLine("✔ Admin auto-créé : admin@gmail.com / admin123");
        }
    }
}
