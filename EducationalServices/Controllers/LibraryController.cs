// EducationalServices.Controllers.LibraryController
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using EducationalServices;

public class LibraryController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly VerificationCodeRepository _verificationRepoCode;
    public LibraryController()
    {
        this.db = new ApplicationDbContext();
        _verificationRepoCode = new VerificationCodeRepository(db);
    }

    // Display available books and library resources
    public ActionResult Index()
    {
        var userId = User.Identity.GetUserId();

        var books = db.Books
            .Include(b => b.Borrows)
            .Include(b => b.Reservations)

            .ToList();


        var viewModel = books.Select(b => new BookViewModel
        {
            BookId = b.BookId,
            Title = b.Title,
            Author = b.Author,
            ImagePath = b.ImagePath,
            IsAvailable = b.IsAvailable,
            Status = b.Status,
            ExpectedReturnDate = b.Borrows
                .Where(br => br.ReturnDate == null)
                .OrderByDescending(br => br.BorrowDate)
                .Select(br => br.ReturnDate)
                .FirstOrDefault(),
            IsReservedByUser = b.Reservations.Any(r => r.UserId == userId)
        }).ToList();

        return View(viewModel);
    }



    [Authorize]
    public ActionResult ReserveBook(int id)
    {
        var userId = User.Identity.GetUserId();
        var book = db.Books
            .Include(b => b.Borrows)
            .Include(b => b.Reservations)
            .FirstOrDefault(b => b.BookId == id);

        if (book == null)
        {
            TempData["Error"] = "Book not found.";
            return RedirectToAction("Index");
        }

        var model = new ReserveBookVM
        {
            BookId = book.BookId,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            ImagePath = book.ImagePath,
        };

        return View(model);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult MarkAsAvailable(int bookId)
    {
        var book = db.Books
            .Include(b => b.Borrows)
            .Include(b => b.Reservations)
            .FirstOrDefault(b => b.BookId == bookId);

        if (book == null)
        {
            TempData["Error"] = "Book not found.";
            return RedirectToAction("Index");
        }

        if(book.Status != BookStatus.UNDER_MAINTENANCE)
        {

            TempData["Success"] = "Failed Book is not Under Maintenance.";
            return RedirectToAction("Index");
        }

        book.Status = BookStatus.AVAILABLE;
        book.IsAvailable = true;
        db.SaveChanges();

        TempData["Success"] = "Book Is now Available for Borrowing.";
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public ActionResult ReserveBook(ReserveBookVM model)
    {
        var userId = User.Identity.GetUserId();
        var book = db.Books
            .Include(b => b.Borrows)
            .Include(b => b.Reservations)
            .FirstOrDefault(b => b.BookId == model.BookId);

        if (book == null)
        {
            TempData["Error"] = "Book not found.";
            return RedirectToAction("Index");
        }

        if (book.Status == BookStatus.RESERVED)
        {
            TempData["Error"] = "This Book has Already been reserved";
            return RedirectToAction("Index");
        }
        else if(book.Status == BookStatus.UNDER_MAINTENANCE)
        {
            TempData["Error"] = "This Book can't be reserved as it is under maintenance";
            return RedirectToAction("Index");
        }
        else if (book.Status == BookStatus.BORROWED)
        {
            TempData["Error"] = "This Book can't be reserved as has been borrowed by someone";
            return RedirectToAction("Index");
        }

        var activeReservationsCount = book.Reservations.Count;
        var reservation = new Reservation
        {
            BookId = model.BookId,
            UserId = userId,
            ReservationDate = model.ReservationStart,
            ReservationOrder = activeReservationsCount + 1,
            Status = ReservationStatus.RESERVED_WAITING
        };


        //generate qrCode
        var code = _verificationRepoCode.Generate(userId: userId, VerificationCodeType.BOOK_RESERVATION);

        reservation.VerificationCodeId = code.Id;
        reservation.VerificationCode = code;


        //save the reservation
        book.Reservations.Add(reservation);
        book.Status = BookStatus.RESERVED;
        book.IsAvailable = false;

        db.SaveChanges();

        TempData["Success"] = $"Book reserved successfully.";
        return RedirectToAction(nameof(ViewReservations));
    }

    [Authorize]
    public ActionResult ViewReservations()
    {
        var userId = User.Identity.GetUserId();
        var reservations = new List<Reservation>();

        if (User.IsInRole("Admin"))
        {
            reservations = db.Reservations
                .Include(r => r.Book)
                .Include(r => r.VerificationCode)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();
        }
        else
        {
            reservations = db.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Book)
                .Include(r => r.VerificationCode)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();
        }

        var viewModel = reservations.Select(r => new ReservationViewModel
        {
            ReservationId = r.ReservationId,
            BookTitle = r.Book.Title,
            BookAuthor = r.Book.Author,
            ReservationDate = r.ReservationDate,
            IsBookAvailable = r.Book.IsAvailable,
            Status = r.Status,
            QRCode = r.VerificationCode?.Base64Img,
            UserId = r.UserId
        }).ToList();

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult CancelReservation(int id)
    {
        var userId = User.Identity.GetUserId();
        var reservation = db.Reservations
            .FirstOrDefault(r => r.ReservationId == id && r.UserId == userId);

        if (reservation == null)
        {
            TempData["Error"] = "Reservation not found or you're not authorized to cancel it.";
            return RedirectToAction("ViewReservations");
        }

        reservation.Book.Status = BookStatus.AVAILABLE;
        reservation.Book.IsAvailable = true;
        reservation.Status = ReservationStatus.CANCELED;
        //db.Reservations.Remove(reservation);
        db.SaveChanges();

        TempData["Success"] = "Reservation cancelled successfully.";
        return RedirectToAction("ViewReservations");
    }

    [HttpGet]
    [Authorize]
    public ActionResult BorrowBook(int bookId)
    {
        
        var book = db.Books
            .Include(t => t.Reservations)
            //include Everything on Reservations

            .Include("Reservations.VerificationCode")
            .Where(t => t.BookId == bookId)
            .FirstOrDefault();

        if (book == null || book.Status != BookStatus.RESERVED)
        {
            TempData["Error"] = "This book is not available for borrowing.";
            return RedirectToAction("Index");
        }

        //check if there are any reservations for this book.
        var reservation = book.Reservations.FirstOrDefault(t => t.Status == ReservationStatus.RESERVED_WAITING);
        if(reservation == null)
        {
            TempData["Error"] = "This book has not been reserved Yet.";
            return RedirectToAction("Index");
        }

        var model = new BorrowViewModel
        {
            BookId = bookId,
            BookTitle = book.Title,
            ExpectedReturnDate = DateTime.UtcNow.AddDays(5),
            ScannedCode = "",
            QRCode = reservation?.VerificationCode?.Base64Img,
            UserId = reservation?.UserId
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> BorrowBook(BorrowViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = model.UserId;

            var book = db.Books.Include(t => t.Reservations).FirstOrDefault(t => t.BookId == model.BookId);

            if (book == null || book.Status != BookStatus.RESERVED)
            {
                TempData["Error"] = "This book is not available for borrowing.";
                return RedirectToAction("Index");
            }

            var reservation = book.Reservations.Where(r => r.UserId == userId && r.Status == ReservationStatus.RESERVED_WAITING).FirstOrDefault();

            if (reservation.ReservationDate.Date < DateTime.UtcNow.Date)
            {
                //If customer has missed the Pick-Up date remove the reservation.
                TempData["Error"] = "Customer has missed His/Her Pick-Up Date";

                reservation.Status = ReservationStatus.EXPIRED;

                book.Status = BookStatus.AVAILABLE;
                book.IsAvailable = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //Verify the code
            if (reservation.VerificationCode.Code != model.ScannedCode)
            {
                TempData["Error"] = "Verification Failed";
                return RedirectToAction("BorrowBook", new { bookId = model.BookId });
            }

            //mark Verification as Verified
            reservation.VerificationCode.Status = VerificationCodeStatus.VERIFIED;

            reservation.Status = ReservationStatus.BOOK_COLLECTED;
            db.Borrows.Add(new Borrow
            {
                BookId = model.BookId,
                UserId = userId,
                BorrowDate = DateTime.Now,
                DueDate = model.ExpectedReturnDate,
                /* ReturnDate = model.ExpectedReturnDate*/ /*Should not set the return date when borrowing*/
            });


            book.IsAvailable = false;
            book.Status = BookStatus.BORROWED;
            /*update the status of the reservation*/
            reservation.Status = ReservationStatus.BOOK_COLLECTED;

            db.SaveChanges();
            TempData["Success"] = "Book has been borrowed Successfully!";
            return RedirectToAction("Index");
        }
        return RedirectToAction("BorrowBook", new { bookId = model.BookId });
    }


    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult ReturnBook(int borrowId)
    {
        var inspection = db.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.BorrowId == borrowId)
                .Select( t => new BookInspectionViewModel
                {
                    BookId = t.BookId,
                    UserId = t.User.Id,
                    BorrowId = t.BorrowId
                }
                ).FirstOrDefault();

        if (inspection == null)
        {
            TempData["Error"] = "Borrow record not found.";
            return RedirectToAction("ViewAllBorrowedBooks");
        }

        if (db.Borrows.FirstOrDefault(t => t.BorrowId == borrowId).ReturnDate.HasValue)
        {
            TempData["Error"] = "This book has already been returned.";
            return RedirectToAction("ViewAllBorrowedBooks");
        }

        return View(inspection);
    }

    


    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ReturnBook(BookInspectionViewModel model)
    {
        try
        {

            #region SaveInspectionSection
            if (ModelState.IsValid)
            {
                var item = new BookInspection
                {
                    BookId = model.BookId,
                    UserId = model.UserId,
                    Status = model.Status,
                    Notes = model.Notes,
                    BorrowId = model.BorrowId,

                };
                 db.BookInspections.Add(item);
            }
            #endregion

            #region ReturnbookSection
            var borrow = db.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .Include(b => b.Book.Reservations)
                .FirstOrDefault(b => b.BorrowId == model.BorrowId);

            if (borrow == null)
            {
                TempData["Error"] = "Borrow record not found.";
                return RedirectToAction("ViewAllBorrowedBooks");
            }

            if (borrow.ReturnDate.HasValue)
            {
                TempData["Error"] = "This book has already been returned.";
                return RedirectToAction("ViewAllBorrowedBooks");
            }

            // Update return date and book availability
            borrow.ReturnDate = DateTime.Now;

            if(model.Status == BookInspectionStatus.DAMAGED)
            {
                borrow.Book.IsAvailable = false;
                borrow.Book.Status = BookStatus.UNDER_MAINTENANCE;
            }
            else 
            {

                borrow.Book.IsAvailable = true;
                borrow.Book.Status = BookStatus.AVAILABLE;
            }

            borrow.IsReturned = true;
            borrow.ReturnDate = DateTime.UtcNow.Date;
            

            db.SaveChanges();
            TempData["Success"] = $"Book '{borrow.Book.Title}' has been returned successfully.";

            #endregion

        }
        catch (Exception)
        {
            throw;
        }

        return RedirectToAction("ViewAllBorrowedBooks");
    }







    // Access digital resources like journals and e-books
    public ActionResult DigitalResources()
    {
        var resources = db.DigitalResources.ToList();
        return View(resources);
    }

    public ActionResult DigitalResourceDetails(int id)
    {
        var resource = db.DigitalResources.Find(id);
        if (resource == null)
        {
            return HttpNotFound();
        }
        return View(resource);
    }

    [Authorize]
    public ActionResult AccessDigitalResource(int id)
    {
        var resource = db.DigitalResources.Find(id);
        if (resource == null)
        {
            return HttpNotFound();
        }
        // Here you might want to log the access or perform any other necessary actions
        return Redirect(resource.FileUrl);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult AddDigitalResource()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public ActionResult AddDigitalResource(DigitalResource resource)
    {
        if (ModelState.IsValid)
        {
            resource.DateAdded = DateTime.Now;
            db.DigitalResources.Add(resource);
            db.SaveChanges();
            return RedirectToAction("DigitalResources");
        }
        return View(resource);
    }





    // Book a study room in the library
    public ActionResult BookStudyRoom(DateTime startTime, int durationInHours)
    {
        var userId = User.Identity.GetUserId();
        db.StudyRoomBookings.Add(new StudyRoomBooking
        {
            UserId = userId,
            StartTime = startTime,
            DurationInHours = durationInHours
        });

        db.SaveChanges();
        TempData["Success"] = "Study room booked successfully!";
        return RedirectToAction("Index");
    }

    // Order a book online for the library
    [Authorize(Roles = "Admin")]
    public ActionResult OrderBook(string bookTitle, string author, decimal price)
    {
        db.BookOrders.Add(new BookOrder
        {
            Title = bookTitle,
            Author = author,
            Price = price,
            OrderDate = DateTime.Now
        });

        db.SaveChanges();
        TempData["Success"] = "Book order placed successfully!";
        return RedirectToAction("Index");
    }

    // Admin receives the ordered book and marks it as available in the library
    [Authorize(Roles = "Admin")]
    public ActionResult ReceiveBook(int orderId)
    {
        var order = db.BookOrders.Find(orderId);

        if (order == null)
        {
            TempData["Error"] = "Invalid order.";
            return RedirectToAction("Index");
        }

        db.Books.Add(new Book
        {
            Title = order.Title,
            Author = order.Author,
            IsAvailable = true
        });

        db.BookOrders.Remove(order);
        db.SaveChanges();
        TempData["Success"] = "Book received and added to the library!";
        return RedirectToAction("Index");
    }

    // Catalog a new book and track its availability in the library
    [Authorize(Roles = "Admin")]
    public ActionResult CatalogBook(string title, string author)
    {
        db.Books.Add(new Book
        {
            Title = title,
            Author = author,
            IsAvailable = true
        });

        db.SaveChanges();
        TempData["Success"] = "Book cataloged successfully!";
        return RedirectToAction("Index");
    }


    [Authorize(Roles = "Admin")]
    public ActionResult AddBook()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public ActionResult AddBook(Book model)
    {
        if (ModelState.IsValid)
        {
            string fileName = null;
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                fileName = Path.GetFileName(model.ImageFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/img/"), fileName);
                model.ImageFile.SaveAs(path);
            }

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                IsAvailable = true,
                ImagePath = fileName
            };

            db.Books.Add(book);
            db.SaveChanges();
            TempData["Success"] = "Book added successfully!";
            return RedirectToAction("Index");
        }
        return View(model);
    }


    [Authorize(Roles = "Admin")]
    public ActionResult ViewAllBooks()
    {
        var allBooks = db.Books.ToList();
        return View(allBooks);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult DeleteBook(int bookId)
    {
        var book = db.Books.Find(bookId);
        if (book == null)
        {
            TempData["Error"] = "Book not found!";
            return RedirectToAction("ViewAllBooks");
        }

        db.Books.Remove(book);
        db.SaveChanges();
        TempData["Success"] = "Book deleted successfully!";
        return RedirectToAction("ViewAllBooks");
    }



    [Authorize(Roles = "Admin")]
    public ActionResult ManageBookOrders()
    {
        var orders = db.BookOrders.ToList();
        return View(orders);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult FulfillOrder(int orderId)
    {
        var order = db.BookOrders.Find(orderId);
        if (order == null)
        {
            TempData["Error"] = "Order not found!";
            return RedirectToAction("ManageBookOrders");
        }

        // Mark the order as fulfilled
        order.IsFulfilled = true;

        // Add the book to the library collection if not already
        db.Books.Add(new Book
        {
            Title = order.Title,
            Author = order.Author,
            IsAvailable = true,

        });

        db.SaveChanges();
        TempData["Success"] = "Order fulfilled, and book added to library stock!";
        return RedirectToAction("ManageBookOrders");
    }


    public ActionResult ViewBorrowedBooks()
    {
        var userId = User.Identity.GetUserId();
        var borrowedBooks = db.Borrows
            .Where(b => b.UserId == userId)
            .Include(b => b.Book)
            .OrderByDescending(b => b.BorrowDate)
            .ToList();
        return View(borrowedBooks);
    }








    public ActionResult ViewBookDetails(int bookId)
    {
        var book = db.Books.Find(bookId);
        if (book == null)
        {
            TempData["Error"] = "Book not found!";
            return RedirectToAction("Index");
        }

        return View(book);
    }



    [Authorize(Roles = "Admin")]
    public ActionResult UpdateBook(int bookId)
    {
        var book = db.Books.Find(bookId);
        if (book == null)
        {
            TempData["Error"] = "Book not found!";
            return RedirectToAction("ViewAllBooks");
        }

        return View(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateBook(Book model)
    {
        if (ModelState.IsValid)
        {
            var book = db.Books.Find(model.BookId);
            if (book == null)
            {
                TempData["Error"] = "Book not found!";
                return RedirectToAction("ViewAllBooks");
            }

            book.Title = model.Title;
            book.Author = model.Author;

            book.IsAvailable = model.IsAvailable;

            db.SaveChanges();
            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction("ViewAllBooks");
        }
        return View(model);
    }


    [Authorize(Roles = "Admin")]
    public ActionResult ViewAllBorrowedBooks(string status = null)
    {
        var query = db.Borrows
            .Include(b => b.Book)
            .Include(b => b.User)
            .Include(b => b.IssueReports)
            .AsQueryable();

        var currentDate = DateTime.Now.Date;

        // Apply filters based on status parameter
        switch (status?.ToLower())
        {
            case "active":
                query = query.Where(b => !b.ReturnDate.HasValue);
                ViewBag.CurrentFilter = "Currently Borrowed";
                break;
            case "overdue":
                query = query.Where(b => !b.ReturnDate.HasValue && b.DueDate < currentDate);
                ViewBag.CurrentFilter = "Overdue";
                break;
            case "returned":
                query = query.Where(b => b.ReturnDate.HasValue);
                ViewBag.CurrentFilter = "Returned";
                break;
            default:
                ViewBag.CurrentFilter = "All Books";
                break;
        }

        var borrowedBooks = query.OrderByDescending(b => b.BorrowDate).ToList();

        // Add summary statistics to ViewBag
        ViewBag.TotalBooks = borrowedBooks.Count;
        ViewBag.OverdueCount = borrowedBooks.Count(b => !b.ReturnDate.HasValue && b.DueDate < currentDate);
        ViewBag.ReturnedCount = borrowedBooks.Count(b => b.ReturnDate.HasValue);
        ViewBag.ActiveCount = borrowedBooks.Count(b => !b.ReturnDate.HasValue);

        return View(borrowedBooks);
    }



    public ActionResult SearchBooks(string query)
    {
        var books = db.Books.AsQueryable();
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
            IsAvailable = b.IsAvailable,
            ImagePath = b.ImagePath
        }).ToList();

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ExtendBorrowing(int borrowId)
    {
        try
        {
            var userId = User.Identity.GetUserId();
            var borrow = db.Borrows
                .Include(b => b.Book)
                .FirstOrDefault(b => b.BorrowId == borrowId && b.UserId == userId);

            if (borrow == null)
            {
                TempData["Error"] = "Borrow record not found.";
                return RedirectToAction("ViewBorrowedBooks");
            }

            var currentDate = DateTime.Now.Date;

            if (borrow.ReturnDate.HasValue)
            {
                // If book was returned, start a new borrowing period
                borrow.ReturnDate = null;
                borrow.BorrowDate = currentDate;
                borrow.DueDate = currentDate.AddDays(14); // Set to 14 days for new borrowing
            }
            else
            {
                // For active borrowings, extend from current due date
                borrow.DueDate = borrow.DueDate < currentDate ?
                    currentDate.AddDays(14) : // If overdue, start fresh
                    borrow.DueDate.AddDays(7); // If not overdue, add 7 days
            }

            db.SaveChanges();
            TempData["Success"] = $"Successfully extended borrowing period for {borrow.Book.Title}. New due date: {borrow.DueDate:MMM dd, yyyy}";
            return RedirectToAction("ViewBorrowedBooks");
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while extending the borrowing period.";
            return RedirectToAction("ViewBorrowedBooks");
        }
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ReportIssue(int borrowId, string issueType, string description)
    {
        try
        {
            var userId = User.Identity.GetUserId();
            var borrow = await db.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId && b.UserId == userId);

            if (borrow == null)
            {
                TempData["Error"] = "Borrow record not found.";
                return RedirectToAction("ViewBorrowedBooks");
            }

            // Create the report
            var report = new BookIssueReport
            {
                BorrowId = borrowId,
                IssueType = issueType,
                Description = description,
                ReportDate = DateTime.Now,
                Status = "Pending",
                UserId = userId,
                BookId = borrow.BookId
            };

            // Add to database
            db.BookIssueReports.Add(report);

            await db.SaveChangesAsync();

            // Send notification to admin (you can implement this based on your notification system)
            await NotifyAdmin(report);

            // Send confirmation to user
            TempData["Success"] = "Your report has been submitted successfully. An administrator will review it shortly.";
            return RedirectToAction("ViewBorrowedBooks");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while submitting the report. Please try again.";
            return RedirectToAction("ViewBorrowedBooks");
        }
    }

    private async Task NotifyAdmin(BookIssueReport report)
    {
        // Implement your admin notification logic here
        // This could be email, internal message, etc.
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



    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> BookReports(string status = null)
    {
        var reports = db.BookIssueReports
            .Include(r => r.Book)
            .Include(r => r.User)
            .Include(r => r.Borrow)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            reports = reports.Where(r => r.Status == status);
        }

        return View(await reports.OrderByDescending(r => r.ReportDate).ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> UpdateBookReport(int reportId, string status, decimal? assessedFee, string adminComments)
    {
        try
        {
            var report = await db.BookIssueReports.FindAsync(reportId);
            if (report == null)
            {
                TempData["Error"] = "Report not found.";
                return RedirectToAction("BookReports");
            }

            report.Status = status;
            report.AssessedFee = assessedFee;
            report.AdminComments = adminComments;

            if (status == "Resolved" && !report.ResolutionDate.HasValue)
            {
                report.ResolutionDate = DateTime.Now;
            }

            await db.SaveChangesAsync();



            TempData["Success"] = "Report updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while updating the report.";
            // Log the error
        }

        return RedirectToAction("BookReports");
    }

}
