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
using Microsoft.AspNetCore.Http;  // <-- IMPORTANT

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
            // 1️⃣  MONGO DB CONFIG
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
            // 2️⃣  SERVICES
            // -------------------------------------------------------
            services.AddSingleton<UserService>();
            services.AddSingleton<DatabaseInitializer>();
            services.AddSingleton<MongoDbService>();
          

     
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<ITpService, TpService>();
            services.AddScoped<IGroupService, GroupService>();

            // ⭐ IMPORTANT
            services.AddScoped<IStudentAnswerService, StudentAnswerService>();
            services.AddScoped<IUserService, UserService>();
            // -------------------------------------------------------
            // 3️⃣  SESSION
            // -------------------------------------------------------
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // -------------------------------------------------------
            // 4️⃣ AUTH COOKIE
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
            // 5️⃣ ROLES
            // -------------------------------------------------------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
            });

            // -------------------------------------------------------
            // 6️⃣ 🔥 IHttpContextAccessor (obligatoire pour @inject)
            // -------------------------------------------------------
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
