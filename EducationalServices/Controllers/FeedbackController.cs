using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;

namespace EducationalServices.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Feedback
        public ActionResult Index()
        {
            return View();
        }

        // GET: Feedback/Create
        public ActionResult Create()
        {
            var feedback = new Feedback();
            return View(feedback);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                feedback.UserId = User.Identity.GetUserId();
                feedback.UserName = User.Identity.Name;
                feedback.DateSubmitted = DateTime.Now;
                feedback.IsResolved = false;

                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("ThankYou");
            }

            return View(feedback);
        }

        // GET: Feedback/ThankYou
        public ActionResult ThankYou()
        {
            return View();
        }

        // GET: Feedback/AdminView
        [Authorize(Roles = "Admin")]
        public ActionResult AdminView()
        {
            var feedbacks = db.Feedbacks.OrderByDescending(f => f.DateSubmitted).ToList();
            return View(feedbacks);
        }

        // POST: Feedback/Resolve
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Resolve(int id, string adminResponse)
        {
            var feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }

            feedback.IsResolved = true;
            feedback.AdminResponse = adminResponse;
            db.SaveChanges();

            return RedirectToAction("AdminView");
        }

        [Authorize]
        public ActionResult MyFeedback()
        {
            var userId = User.Identity.GetUserId();
            var feedbacks = db.Feedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.DateSubmitted)
                .ToList();
            return View(feedbacks);
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
}
