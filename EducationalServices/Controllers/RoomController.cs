using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using EducationalServices.Models;


public class RoomController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    // GET: Room
    public ActionResult Index()
    {
        var rooms = db.Rooms.ToList();

        // Update availability based on current bookings
        var now = DateTime.Now;
        foreach (var room in rooms)
        {
            room.IsAvailable = !db.RoomBookings.Any(rb => rb.RoomId == room.RoomId
                                                       && rb.StartTime <= now
                                                       && rb.EndTime > now);
        }

        return View(rooms);
    }

    // GET: Room/Create
    [Authorize(Roles = "Admin")]
    public ActionResult Create()
    {
        return View();
    }

    // POST: Room/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult Create(Room room)
    {
        if (ModelState.IsValid)
        {
            db.Rooms.Add(room);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(room);
    }

    // GET: Room/Edit/5
    [Authorize(Roles = "Admin")]
    public ActionResult Edit(int id)
    {
        var room = db.Rooms.Find(id);
        if (room == null)
        {
            return HttpNotFound();
        }
        return View(room);
    }

    // POST: Room/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult Edit(Room room)
    {
        if (ModelState.IsValid)
        {
            db.Entry(room).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(room);
    }


    public ActionResult Schedule(int id, DateTime? date)
    {
        var room = db.Rooms.Find(id);
        if (room == null)
        {
            return HttpNotFound();
        }

        var currentDate = date ?? DateTime.Today;
        var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);

        var bookings = db.RoomBookings
            .Where(rb => rb.RoomId == id && rb.StartTime >= weekStart && rb.StartTime < weekEnd)
            .Include(rb => rb.User)
            .OrderBy(rb => rb.StartTime)
            .ToList();

        var now = DateTime.Now;
        room.IsAvailable = !bookings.Any(b => b.StartTime <= now && b.EndTime > now);

        ViewBag.Room = room;
        ViewBag.CurrentDate = currentDate;
        ViewBag.IsCurrentlyAvailable = room.IsAvailable;

        return View(bookings);
    }







    // POST: Room/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult DeleteConfirmed(int id)
    {
        Room room = db.Rooms.Find(id);
        db.Rooms.Remove(room);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
    // GET: Room/Book/5
    [Authorize]
    public ActionResult Book(int id)
    {
        var room = db.Rooms.Find(id);
        if (room == null)
        {
            return HttpNotFound();
        }
        return View(new RoomBookingViewModel { RoomId = id, RoomName = room.RoomName });
    }

    // POST: Room/Book
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult Book(RoomBookingViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.Identity.GetUserId();
            var booking = new RoomBooking
            {
                RoomId = model.RoomId,
                UserId = userId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                IsActive = true
            };

            db.RoomBookings.Add(booking);
            db.SaveChanges();

            return RedirectToAction("MyBookings");
        }

        return View(model);
    }

    // GET: Room/MyBookings
    [Authorize]
    public ActionResult MyBookings()
    {
        var userId = User.Identity.GetUserId();
        var bookings = db.RoomBookings
            .Where(rb => rb.UserId == userId && rb.IsActive)
            .Include(rb => rb.Room)
            .ToList();

        return View(bookings);
    }

    // POST: Room/ExtendBooking
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult ExtendBooking(int id, int extensionMinutes)
    {
        var booking = db.RoomBookings.Find(id);
        if (booking == null || booking.UserId != User.Identity.GetUserId())
        {
            return HttpNotFound();
        }

        booking.EndTime = booking.EndTime.AddMinutes(extensionMinutes);
        db.SaveChanges();

        return RedirectToAction("MyBookings");
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult CancelBooking(int id)
    {
        var booking = db.RoomBookings.Find(id);
        if (booking == null || booking.UserId != User.Identity.GetUserId())
        {
            return HttpNotFound();
        }

        booking.IsActive = false;
        db.SaveChanges();

        return RedirectToAction("MyBookings");
    }



    [HttpGet]
    public ActionResult SearchBooks(string query)
    {
        var userId = User.Identity.GetUserId();
        var books = db.Books
            .Include(b => b.Borrows)
            .Include(b => b.Reservations)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            books = books.Where(b =>
                b.Title.Contains(query) ||
                b.Author.Contains(query));
        }

        var viewModel = books.Select(b => new BookViewModel
        {
            BookId = b.BookId,
            Title = b.Title,
            Author = b.Author,
            ImagePath = b.ImagePath,
            IsAvailable = b.IsAvailable,
            IsReservedByUser = b.Reservations.Any(r => r.UserId == userId)
        }).ToList();

        return View(viewModel);
    }


    [Authorize]
        public ActionResult ExtendBorrowing(int borrowId)
        {
            var borrow = db.Borrows.Find(borrowId);
            if (borrow?.UserId != User.Identity.GetUserId())
                return HttpNotFound();
            borrow.ReturnDate = borrow.ReturnDate?.AddDays(7);
            db.SaveChanges();
            return RedirectToAction("ViewBorrowedBooks");
        }

        [Authorize]
        public ActionResult GetBookReminders()
        {
            var userId = User.Identity.GetUserId();
            var dueSoonBooks = db.Borrows
                .Where(b => b.UserId == userId && b.ReturnDate <= DateTime.Now.AddDays(3))
                .Include(b => b.Book)
                .ToList();
            return Json(dueSoonBooks, JsonRequestBehavior.AllowGet);
        }
    }


