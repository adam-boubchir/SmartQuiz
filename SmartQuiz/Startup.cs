using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using SmartQuiz.Services;
using System;
using System.Threading.Tasks;
using YourProject.Services;

namespace SmartQuiz
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // -------------------------------------------------------
            // 🔥 1. MONGO DB CONFIGURATION CORRECTE
            // -------------------------------------------------------
            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(Configuration["MongoDB:ConnectionString"])
            );

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(Configuration["MongoDB:DatabaseName"]);
            });

            // -------------------------------------------------------
            // 🔥 2. SERVICES (User + Quiz + TP + Group)
            // -------------------------------------------------------
            services.AddSingleton<UserService>();
            services.AddSingleton<DatabaseInitializer>();
            services.AddSingleton<MongoDbService>();

            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<ITpService, TpService>();
            services.AddScoped<IGroupService, GroupService>();

            // -------------------------------------------------------
            // 🔥 3. SESSIONS
            // -------------------------------------------------------
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // -------------------------------------------------------
            // 🔥 4. AUTH COOKIE
            // -------------------------------------------------------
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                });

            // -------------------------------------------------------
            // 🔥 5. ROLES
            // -------------------------------------------------------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseInitializer dbInit)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Création de l’admin
            Task.Run(async () => await dbInit.SeedAdminUser());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
