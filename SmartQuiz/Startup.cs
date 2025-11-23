using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartQuiz.Services;
using System;
using System.Threading.Tasks;

namespace SmartQuiz
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // -------------------------------------------------------
        // CONFIGURE SERVICES
        // -------------------------------------------------------
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // ---------- MongoDB Services ----------
            services.AddSingleton<UserService>();
            services.AddSingleton<DatabaseInitializer>();

            // ---------- Sessions ----------
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ---------- Authentification Cookie ----------
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                });

            // ---------- Politique d'accès par rôle ----------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole("Teacher"));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("StudentOnly", policy =>
                    policy.RequireRole("Student"));
            });
        }

        // -------------------------------------------------------
        // CONFIGURE PIPELINE
        // -------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseInitializer dbInit)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // ---------- Création automatique du compte Admin ----------
            Task.Run(async () => await dbInit.SeedAdminUser());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ⚠️ Obligatoire avant auth
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Route MVC classique
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
