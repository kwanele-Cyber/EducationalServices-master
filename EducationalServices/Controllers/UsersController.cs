// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.UsersController
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

public class UsersController : Controller
{
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Create(User users)
    {
        string role = users.Role.ToString();
        string email = users.Email;
        string password = users.Password;
        ApplicationDbContext db = new ApplicationDbContext();
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        try
        {
            if (!roleManager.RoleExists(role))
            {
                roleManager.Create(new IdentityRole(role));
            }
            ApplicationUser user = new ApplicationUser();
            user.UserName = email;
            user.Email = email;
            user.EmailConfirmed = true;
            string pwd = password;
            IdentityResult newuser = userManager.Create(user, pwd);
            if (newuser.Succeeded)
            {
                userManager.AddToRole(user.Id, role);
            }
            base.TempData["Success"] = "User created successfully!!!";
        }
        catch
        {
            base.TempData["Error"] = "Something went wrong, Please try again later.";
            return View();
        }
        return View();
    }
}
