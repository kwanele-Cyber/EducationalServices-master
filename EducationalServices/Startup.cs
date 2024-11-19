using System;
using System.Data.Entity;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(EducationalServices.Startup))]

namespace EducationalServices
{
    public partial class Startup

    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureSignalR(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<IVerificationCodeRepository<VerificationCode>>(VerificationCodeRepository.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie("ExternalCookie");
            app.UseTwoFactorSignInCookie("TwoFactorCookie", TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie("TwoFactorRememberBrowser");

            CreateRolesAndUsers();
        }

        private void ConfigureSignalR(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true
            };
            app.MapSignalR(hubConfiguration);
        }

        private void CreateRolesAndUsers()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (!roleManager.RoleExists("Admin"))
                {
                    var role = new IdentityRole("Admin");
                    roleManager.Create(role);

                    var user = new ApplicationUser
                    {
                        UserName = "Admin@gmail.com",
                        Email = "Admin@gmail.com",
                        FirstName = "Spider",
                        LastName = "Man"
                    };

                    string password = "Admin@111";
                    var result = userManager.Create(user, password);

                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }

                if (!roleManager.RoleExists("Tutor"))
                {
                    var role = new IdentityRole("Tutor");
                    roleManager.Create(role);
                }
            }
        }
    }
}
