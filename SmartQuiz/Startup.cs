using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using SmartQuiz.Hubs;
using SmartQuiz.Services;
using System;
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
            // 1️⃣ MONGO DB
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
            // 2️⃣ CORE SERVICES
            // -------------------------------------------------------
            services.AddSingleton<MongoDbService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<ITpService, TpService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IStudentProfileService, StudentProfileService>();
            services.AddScoped<IStudentAnswerService, StudentAnswerService>();

            // 👉 AJOUT MANQUANT (FIX DE TON ERREUR)
            services.AddScoped<IStudentService, StudentService>();

            // -------------------------------------------------------
            // 3️⃣ CHAT SERVICES
            // -------------------------------------------------------
            services.AddSignalR();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatContactService, ChatContactService>();

            // -------------------------------------------------------
            // 4️⃣ SESSION
            // -------------------------------------------------------
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // -------------------------------------------------------
            // 5️⃣ COOKIES AUTH
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
            // 6️⃣ ROLES
            // -------------------------------------------------------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", p => p.RequireRole("Teacher"));
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("StudentOnly", p => p.RequireRole("Student"));
            });

            // -------------------------------------------------------
            // 7️⃣ HTTP CONTEXT ACCESSOR
            // -------------------------------------------------------
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // -------------------------------------------------------
            // 8️⃣ DATABASE INITIALIZER (IMPORTANT)
            // -------------------------------------------------------
            services.AddScoped<DatabaseInitializer>();
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

            // Seed initial admin user (synchronisé)
            dbInit.SeedAdminUser().Wait();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
