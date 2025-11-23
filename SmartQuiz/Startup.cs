using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartQuiz.Services;
using System;

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

            // ---------- Authentification Cookie ----------
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";       // Utiliser AccountController
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                });

            // ---------- Politique d'accès par rôle (optionnelle) ----------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole("Teacher"));
            });

            // ---------- Sessions ----------
            services.AddDistributedMemoryCache(); // nécessaire pour la session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        // -------------------------------------------------------
        // CONFIGURE HTTP PIPELINE
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

            // Seed automatique admin
            Task.Run(async () => await dbInit.SeedAdminUser());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ⚠️ Les sessions doivent être avant l'authentification
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Route pour les Area (Teacher)
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

                // Route par défaut
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
