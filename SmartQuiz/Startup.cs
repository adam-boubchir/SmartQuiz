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

            // Services MongoDB
            services.AddSingleton<UserService>();
            services.AddSingleton<DatabaseInitializer>();   // Seeder Admin

            // Authentification Cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";

                    // Empêche les redirections infinies
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(6);
                });

            // Sessions
            services.AddSession();
        }

        // -------------------------------------------------------
        // CONFIGURE - pipeline HTTP
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

            // -------------------------------------------------------
            // 🌟 SEED ADMIN AUTOMATIQUE
            // -------------------------------------------------------
            Task.Run(async () => await dbInit.SeedAdminUser());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
