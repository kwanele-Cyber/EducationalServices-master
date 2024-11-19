// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.DriversController
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EducationalServices.Models;

public class DriversController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult Index(int? orderId)
    {
        if (orderId > 0)
        {
            base.Session["CustOrdID"] = orderId.ToString();
        }
        return View(db.Drivers.ToList());
    }

    public ActionResult MyProfile(string email = "Email")
    {
        if (!(email == "Email"))
        {
            base.ViewBag.Title = "Single";
        }
        else
        {
            email = base.User.Identity.Name;
            base.ViewBag.Title = "MyProfile";
        }
        return View(db.Drivers.Where((Driver x) => x.Email == email).ToList());
    }

    public ActionResult Details(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Driver driver = db.Drivers.Find(id);
        if (driver == null)
        {
            return HttpNotFound();
        }
        return View(driver);
    }

    public ActionResult Create()
    {
        Driver b = new Driver
        {
            Email = base.User.Identity.Name
        };
        return View(b);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "DrivId,Name,Surname,Email,Picture,IsAvailable,CarName,CarModel,CarReg,CarType,PhoneNumber,Address,Capacity")] Driver driver, HttpPostedFileBase file)
    {
        if (base.ModelState.IsValid)
        {
            string pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string picturePath = Path.Combine(base.Server.MapPath("~/images/"), pictureFileName);
            file.SaveAs(picturePath);
            driver.Picture = pictureFileName;
            driver.IsAvailable = true;
            db.Drivers.Add(driver);
            db.SaveChanges();
            return RedirectToAction("MyProfile");
        }
        return View(driver);
    }

    public ActionResult Edit(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Driver driver = db.Drivers.Find(id);
        if (driver == null)
        {
            return HttpNotFound();
        }
        return View(driver);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "DrivId,Name,Surname,Email,Picture,IsAvailable,CarName,CarModel,CarReg,CarType,PhoneNumber,Address")] Driver driver)
    {
        if (base.ModelState.IsValid)
        {
            db.Entry(driver).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(driver);
    }

    public ActionResult Delete(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Driver driver = db.Drivers.Find(id);
        if (driver == null)
        {
            return HttpNotFound();
        }
        return View(driver);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Driver driver = db.Drivers.Find(id);
        db.Drivers.Remove(driver);
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
}
