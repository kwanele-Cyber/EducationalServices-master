// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.SupportController
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;

public class SupportController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(SupportRequest supportRequest)
    {
        if (base.ModelState.IsValid)
        {
            supportRequest.StudentId = base.User.Identity.GetUserId();
            supportRequest.CreatedAt = DateTime.Now;
            supportRequest.Status = "Open";
            db.SupportRequests.Add(supportRequest);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        return View(supportRequest);
    }

    [Authorize(Roles = "Admin,Tutor")]
    public ActionResult Index()
    {
        List<SupportRequest> requests = db.SupportRequests.Include((SupportRequest s) => s.Student).ToList();
        return View(requests);
    }

    [Authorize(Roles = "Admin,Tutor")]
    public ActionResult Resolve(int id)
    {
        SupportRequest request = db.SupportRequests.Find(id);
        if (request == null)
        {
            return HttpNotFound();
        }
        return View(request);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Tutor")]
    [ValidateAntiForgeryToken]
    public ActionResult Resolve(int id, string resolution)
    {
        SupportRequest request = db.SupportRequests.Find(id);
        if (request == null)
        {
            return HttpNotFound();
        }
        request.Status = "Resolved";
        request.Resolution = resolution;
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
