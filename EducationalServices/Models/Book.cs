using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace EducationalServices.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public bool IsAvailable { get; set; }

        public string ImagePath { get; set; }

        public BookStatus Status { get; set; } // Added Status property

        [NotMapped]
        [Display(Name = "Book Cover")]
        public HttpPostedFileBase ImageFile { get; set; }



        public virtual ICollection<Borrow> Borrows { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<BookIssueReport> IssueReports { get; set; }


        public Book()
        {
            Borrows = new HashSet<Borrow>();
            Reservations = new HashSet<Reservation>();
            IssueReports = new HashSet<BookIssueReport>();
            Status = BookStatus.AVAILABLE;
        }
    }
}