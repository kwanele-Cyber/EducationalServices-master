using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using EducationalServices.Models;
using EducationalServices;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.IO;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Threading.Tasks;


public class RoomController : Controller
{
    private ApplicationDbContext db;

    private VerificationCodeRepository _verificationRepoCode;

    public RoomController()
    {
        db = new ApplicationDbContext();
        _verificationRepoCode = new VerificationCodeRepository(db);
    }


    // GET: Room
    public async Task<ActionResult> Index()
    {
        var now = DateTime.Now;

        var rooms = await db.Rooms
            .Include(r => r.RoomBookings)
            .ToListAsync();

        foreach (var room in rooms)
        {
            room.IsAvailable = !room.RoomBookings.Any(b =>
                (b.StartTime <= now && b.EndTime > now) || // Active booking
                (b.CheckInTime.HasValue && b.EndTime > now)); // Checked-in booking
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
            room.IsAvailable = true;
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


    public async Task<ActionResult> Schedule(int id, DateTime? date)
    {
        var room = await db.Rooms.FindAsync(id);
        if (room == null)
        {
            return HttpNotFound();
        }

        var currentDate = date ?? DateTime.Today;
        var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);

        var bookings = await db.RoomBookings
            .Where(rb => rb.RoomId == id && rb.StartTime >= weekStart && rb.StartTime < weekEnd)
            .Include(rb => rb.User)
            .OrderBy(rb => rb.StartTime)
            .ToListAsync();

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
    public async Task<ActionResult> Book(RoomBookingViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.Identity.GetUserId();

        // Rounds time to the nearest 5-minute interval
        DateTime RoundToNearestFiveMinutes(DateTime time)
        {
            var totalMinutes = time.Hour * 60 + time.Minute;
            var roundedMinutes = (int)Math.Round(totalMinutes / 5.0) * 5;
            return new DateTime(time.Year, time.Month, time.Day, roundedMinutes / 60, roundedMinutes % 60, 0);
        }

        model.StartTime = RoundToNearestFiveMinutes(model.StartTime);
        model.EndTime = RoundToNearestFiveMinutes(model.EndTime);

        // Check for conflicting bookings in the database
        var hasConflict = await db.RoomBookings
            .AnyAsync(t =>
                t.RoomId == model.RoomId &&
                t.IsActive &&
                t.EndTime > DateTime.Now && // Consider only active, future bookings
                (model.StartTime < t.EndTime && model.EndTime > t.StartTime)); // Overlap check

        if (hasConflict)
        {
            TempData["Error"] = "The selected time slot conflicts with an existing room reservation.";
            ModelState.AddModelError(string.Empty, "The selected time slot conflicts with an existing room reservation.");
            return View(model);
        }

        // Prepare QR code data as JSON
        var qrCodePayload = new
        {
            roomId = model.RoomId,
            userId = userId,
            startTime = model.StartTime,
            endTime = model.EndTime
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(qrCodePayload);

        // Generate QR code
        var code = _verificationRepoCode.Generate(
            userId: userId,
            Type: VerificationCodeType.ROOM_BOOKING,
            Message: jsonPayload
        );

        // Create a new room booking
        var booking = new RoomBooking
        {
            RoomId = model.RoomId,
            UserId = userId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            IsActive = true,
            VerificationId = code.Id.ToString()
        };

        db.RoomBookings.Add(booking);
        await db.SaveChangesAsync();

        TempData["Success"] = "Room booking successful.";
        return RedirectToAction("MyBookings");
    }


    [Authorize]
    public async Task<ActionResult> CheckIn(int id)
    {
        // Retrieve only necessary fields to optimize performance
        var booking = await db.RoomBookings
            .Where(t => t.RoomBookingId == id)
            .Select(t => new
            {
                t.VerificationCode.Base64Img,
                t.StartTime,
                t.EndTime
            })
            .FirstOrDefaultAsync();

        // Handle case where booking is not found
        if (booking == null)
        {
            TempData["Error"] = "Booking not found.";
            return RedirectToAction("MyBookings");
        }

        // Use ViewData instead of ViewBag for slightly improved performance
        ViewData["QRCodeImage"] = booking.Base64Img;
        ViewData["StartTime"] = booking.StartTime;
        ViewData["EndTime"] = booking.EndTime;

        return View();
    }


    // Display the Verify Check-In view for admin
    [Authorize(Roles = "Admin")]
    public ActionResult VerifyCheckIn()
    {
        return View();
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ProcessCheckInQRCode(string scannedContent)
    {
        try
        {
            // Deserialize the scanned QR code content into the model
            var scannedResult = JsonConvert.DeserializeObject<CheckInScanResult>(scannedContent);

            if (scannedResult == null)
            {
                // Handle invalid scanned content gracefully
                ViewData["Error"] = "Invalid QR code data.";
                return RedirectToAction(nameof(VerifyCheckIn));
            }

            // Use a more optimized query and avoid unnecessary LINQ filtering on large datasets
            var booking = await db.RoomBookings
                .Where(t => t.RoomId == scannedResult.roomId &&
                            t.UserId == scannedResult.userId &&
                            t.StartTime == scannedResult.startTime &&
                            t.EndTime == scannedResult.endTime &&
                            t.IsActive)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                ViewData["Error"] = "Booking not found or invalid.";
                return RedirectToAction(nameof(VerifyCheckIn));
            }

            var today = DateTime.Now;

            // Check if the booking start time is valid for check-in (within 15 minutes of the start time)
            var checkInWindowStart = booking.StartTime.AddMinutes(-15);
            var checkInWindowEnd = booking.EndTime;

            if (today < checkInWindowStart || today > checkInWindowEnd)
            {
                ViewData["Error"] = "Check-in time has not arrived or has passed.";
                return RedirectToAction(nameof(Index));
            }

            // Check if the user has already checked in
            if (booking.CheckInTime.HasValue)
            {
                ViewData["Error"] = "User has already checked in.";
                return RedirectToAction(nameof(Index));
            }

            // Proceed with check-in (set the CheckInTime)
            booking.CheckInTime = today;
            db.SaveChanges();

            ViewData["Success"] = "Check-in successful.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the exception if necessary and handle gracefully
            ViewData["Error"] = "An error occurred while processing the check-in.";
            return RedirectToAction(nameof(VerifyCheckIn));
        }
    }


    // GET: Room/MyBookings
    [Authorize]
    public async Task<ActionResult> MyBookings()
    {
        var userId = User.Identity.GetUserId();

        // Use projection to retrieve only the required fields to minimize memory usage and improve performance
        var bookings = await db.RoomBookings
            .Where(rb => rb.UserId == userId && rb.IsActive)
            .Select(rb => new
            {
                rb.RoomBookingId,
                rb.Room.RoomName,
                rb.StartTime,
                rb.EndTime,
                rb.CheckInTime,
                rb.VerificationCode.Base64Img,
                rb.RoomId,
                rb.UserId
            })
            .ToListAsync();

        // Map the projection results to a model for the view
        var bookingViewModel = bookings.Select(booking => new MyBookingsViewModel
        {
            RoomBookingId = booking.RoomBookingId,
            RoomName = booking.RoomName,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            CheckInTime = booking.CheckInTime,
            Base64Img = booking.Base64Img,
            RemainingTime = booking.EndTime > DateTime.Now
                ? $"{(booking.EndTime - DateTime.Now).Hours} hours {(booking.EndTime - DateTime.Now).Minutes} minutes"
                : "Expired"
        }).ToList();

        return View(bookingViewModel);
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ExtendBooking(int id, int extensionMinutes)
    {
        if (extensionMinutes <= 0)
        {
            TempData["Error"] = "Extension time must be greater than zero.";
            return RedirectToAction("MyBookings");
        }

        if (extensionMinutes < 60)
        {
            TempData["Error"] = "Extension time must be not less than 60.";
            return RedirectToAction("MyBookings");
        }

        var booking = await db.RoomBookings.FindAsync(id);
        if (booking == null || booking.UserId != User.Identity.GetUserId())
        {
            return HttpNotFound();
        }

        var newEndTime = booking.EndTime.AddMinutes(extensionMinutes);

        // Check for overlapping bookings for the same room
        var hasConflict = db.RoomBookings.Any(b =>
            b.RoomId == booking.RoomId && 
            b.IsActive && 
            b.RoomBookingId != booking.RoomBookingId && 
            b.StartTime <= newEndTime && 
            b.EndTime >= booking.EndTime
        );

        if (hasConflict)
        {
            TempData["Error"] = "Extension overlaps with an existing booking.";
            return RedirectToAction("MyBookings");
        }

        booking.EndTime = newEndTime;
        await db.SaveChangesAsync();

        TempData["Success"] = "Booking extended successfully.";
        return RedirectToAction("MyBookings");
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> CancelBooking(int id)
    {
        var booking = await db.RoomBookings.FindAsync(id);
        if (booking == null || booking.UserId != User.Identity.GetUserId())
        {
            return HttpNotFound();
        }

        booking.IsActive = false;
        await db.SaveChangesAsync();

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
        }).ToListAsync().Result;

        return View(viewModel);
    }


    [Authorize]
    public async Task<ActionResult> ExtendBorrowing(int borrowId)
    {
        var borrow = await db.Borrows.FindAsync(borrowId);

        if (borrow?.UserId != User.Identity.GetUserId())
            return HttpNotFound();


        borrow.ReturnDate = borrow.ReturnDate?.AddDays(7);
        await db.SaveChangesAsync();

        return RedirectToAction("ViewBorrowedBooks");
    }

    [Authorize]
    public async Task<ActionResult> GetBookReminders()
    {
        var userId = User.Identity.GetUserId();
        var dueSoonBooks = await db.Borrows
            .Where(b => b.UserId == userId && b.ReturnDate <= DateTime.Now.AddDays(3))
            .Include(b => b.Book)
            .ToListAsync();

        return Json(dueSoonBooks, JsonRequestBehavior.AllowGet);
    }
}


