using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;   // AJOUT
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization; // AJOUT
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

            // AJOUT : Service Teacher
            services.AddSingleton<TeacherService>();

            // ---------- Authentification ----------
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                });

            // AJOUT : politique d'accès par rôle
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole("teacher"));
            });

            // Sessions
            services.AddSession();
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            // ----------- ROUTING AVEC AREA ----------
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
