// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.ModulesController
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;

public class ModulesController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    [HttpPost]
    public ActionResult Cancel()
    {
        base.TempData["Cancel"] = "Your Order Has Been Cancelled";
        return RedirectToAction("RegisterForModule");
    }

    [HttpGet]
    public ActionResult RegisteredStudent()
    {
        string userId = base.User.Identity.GetUserId();
        List<StudentModule> studentModules;
        if (base.User.IsInRole("Admin"))
        {
            studentModules = db.StudentModules.Include((StudentModule sm) => sm.Module).ToList();
            return View(studentModules);
        }
        studentModules = (from x in db.StudentModules.Include((StudentModule sm) => sm.Module)
                          where x.StudentId == userId
                          select x).ToList();
        return View(studentModules);
    }

    [HttpGet]
    public ActionResult RegisterForModule()
    {
        List<Module> modules = db.Modules.ToList();
        return View(modules);
    }

    [HttpGet]
    public ActionResult Payment(int moduleId, bool isRecommendation = false)
    {
        Module modules = db.Modules.Where((Module x) => x.ModuleId == moduleId).FirstOrDefault();
        base.ViewBag.IsRecommendation = isRecommendation;
        return View(modules);
    }

    [HttpGet]
    public ActionResult MakePayment(int? CardId, int ModuleId, decimal Price)
    {
        base.ViewBag.ModuleId = ModuleId;
        base.ViewBag.Price = Price;
        Card Card = db.Cards.Where((Card x) => (int?)x.CardId == CardId).FirstOrDefault();
        return View(Card);
    }

    [HttpPost]
    public ActionResult MakePayment(int ModuleId, bool isRecommendation = false)
    {
        Module module = db.Modules.Find(ModuleId);
        if (module == null)
        {
            return HttpNotFound();
        }
        string userId = base.User.Identity.GetUserId();
        StudentModule existingEnrollment = db.StudentModules.FirstOrDefault((StudentModule sm) => sm.StudentId == userId && sm.ModuleId == ModuleId);
        if (existingEnrollment != null)
        {
            base.TempData["Info"] = "You are already enrolled in this module.";
            return RedirectToAction("RegisteredStudent");
        }
        StudentModule studentModule = new StudentModule
        {
            ModuleId = ModuleId,
            StudentId = userId,
            Status = "Paid",
            TotalPaid = module.Price
        };
        db.StudentModules.Add(studentModule);
        if (isRecommendation)
        {
            CourseRecommendation recommendation = db.CourseRecommendations.FirstOrDefault((CourseRecommendation cr) => cr.StudentId == userId && cr.ModuleId == ModuleId);
            if (recommendation != null)
            {
                db.CourseRecommendations.Remove(recommendation);
            }
        }
        db.SaveChanges();
        base.TempData["Success"] = "Your enrollment has been completed. Thank you for your payment!";
        return RedirectToAction("RegisteredStudent");
    }

    public ActionResult Index()
    {
        return View(db.Modules.ToList());
    }

    public ActionResult Details(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Module module = db.Modules.Find(id);
        if (module == null)
        {
            return HttpNotFound();
        }
        return View(module);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "ModuleId,Subject,SubjectCode,Description,DurationInHours,Difficulty,Price")] Module module)
    {
        if (base.ModelState.IsValid)
        {
            db.Modules.Add(module);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(module);
    }

    public ActionResult Edit(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Module module = db.Modules.Find(id);
        if (module == null)
        {
            return HttpNotFound();
        }
        return View(module);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "ModuleId,Subject,SubjectCode,Description,DurationInHours,Difficulty,Price")] Module module)
    {
        if (base.ModelState.IsValid)
        {
            db.Entry(module).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(module);
    }

    public ActionResult Delete(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Module module = db.Modules.Find(id);
        if (module == null)
        {
            return HttpNotFound();
        }
        return View(module);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Module module = db.Modules.Find(id);
        db.Modules.Remove(module);
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

    public ActionResult TrackProgress(string studentId)
    {
        var progress = (from sm in db.StudentModules
                        where sm.StudentId == studentId
                        select new
                        {
                            Module = sm.Module,
                            Quizzes = sm.Module.Quizzes.Select((Quiz q) => new
                            {
                                Quiz = q,
                                Attempts = q.Attempts.Where((QuizAttempt a) => a.StudentId == studentId)
                            })
                        }).ToList();
        return View(progress);
    }
}
