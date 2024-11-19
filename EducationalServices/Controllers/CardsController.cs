// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.CardsController
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;

public class CardsController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult Index()
    {
        return View(db.Cards.ToList());
    }

    public ActionResult Details(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Card card = db.Cards.Find(id);
        if (card == null)
        {
            return HttpNotFound();
        }
        return View(card);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "CardId,Name,Number,ExpirationDate,UserId")] Card card)
    {
        if (base.ModelState.IsValid)
        {
            card.UserId = base.User.Identity.GetUserId();
            db.Cards.Add(card);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(card);
    }

    public ActionResult Edit(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Card card = db.Cards.Find(id);
        if (card == null)
        {
            return HttpNotFound();
        }
        return View(card);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "CardId,Name,Number,ExpirationDate,UserId")] Card card)
    {
        if (base.ModelState.IsValid)
        {
            card.UserId = base.User.Identity.GetUserId();
            db.Entry(card).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(card);
    }

    public ActionResult Delete(int? id)
    {
        if (!id.HasValue)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Card card = db.Cards.Find(id);
        if (card == null)
        {
            return HttpNotFound();
        }
        return View(card);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Card card = db.Cards.Find(id);
        db.Cards.Remove(card);
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
