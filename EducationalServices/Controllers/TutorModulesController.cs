// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.TutorModulesController
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

public class TutorModulesController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult Index()
    {
        IQueryable<TutorModule> tutorModules;
        if (base.User.IsInRole("Admin"))
        {
            tutorModules = db.TutorModules.Include((TutorModule t) => t.Module).Include((TutorModule x) => x.User);
            return View(tutorModules.ToList());
        }
        string UserId = base.User.Identity.GetUserId();
        tutorModules = from x in db.TutorModules.Include((TutorModule t) => t.Module).Include((TutorModule x) => x.User)
                       where x.TutorId == UserId
                       select x;
        return View(tutorModules.ToList());
    }

    public ActionResult Details(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        TutorModule tutorModule = db.TutorModules.Find(id);
        if (tutorModule == null)
        {
            return HttpNotFound();
        }
        return View(tutorModule);
    }

    public ActionResult Create()
    {
        base.ViewBag.ModuleId = new SelectList(db.Modules, "ModuleId", "Subject");
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        IdentityRole tutorRole = db.Roles.SingleOrDefault((IdentityRole r) => r.Name == "Tutor");
        List<string> tutorRoleUsers = tutorRole.Users.Select((IdentityUserRole ru) => ru.UserId).ToList();
        List<ApplicationUser> tutors = userManager.Users.Where((ApplicationUser u) => tutorRoleUsers.Contains(u.Id)).ToList();
        base.ViewBag.Tutors = tutors;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "TutorModuleId,ModuleId,TutorId")] TutorModule tutorModule)
    {
        if (base.ModelState.IsValid)
        {
            db.TutorModules.Add(tutorModule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        base.ViewBag.ModuleId = new SelectList(db.Modules, "ModuleId", "Subject", tutorModule.ModuleId);
        return View(tutorModule);
    }

    public ActionResult Edit(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        TutorModule tutorModule = db.TutorModules.Find(id);
        if (tutorModule == null)
        {
            return HttpNotFound();
        }
        base.ViewBag.ModuleId = new SelectList(db.Modules, "ModuleId", "Subject", tutorModule.ModuleId);
        return View(tutorModule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "TutorModuleId,ModuleId,TutorId")] TutorModule tutorModule)
    {
        if (base.ModelState.IsValid)
        {
            db.Entry(tutorModule).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        base.ViewBag.ModuleId = new SelectList(db.Modules, "ModuleId", "Subject", tutorModule.ModuleId);
        return View(tutorModule);
    }

    public ActionResult Delete(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        TutorModule tutorModule = db.TutorModules.Find(id);
        if (tutorModule == null)
        {
            return HttpNotFound();
        }
        return View(tutorModule);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        TutorModule tutorModule = db.TutorModules.Find(id);
        db.TutorModules.Remove(tutorModule);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }

    public ActionResult RecommendCourse(string studentId)
    {
        ApplicationUser student = db.Users.Find(studentId);
        List<StudentModule> studentModules = db.StudentModules.Where((StudentModule sm) => sm.StudentId == studentId).ToList();
        return View(new RecommendationViewModel
        {
            Student = student,
            Modules = studentModules
        });
    }
}
