using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EducationalServices.Models
{
    public class Borrow
    {
        public int BorrowId { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateTime BorrowDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; } = false;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
        public DateTime DueDate { get; set; }

        // Navigation properties
        public virtual Book Book { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<BorrowExtension> BorrowExtensions { get; set; }
          public virtual ICollection<BookIssueReport> IssueReports { get; set; }

        public Borrow()
        {
            var now = DateTime.Now;
            BorrowDate = now;
            DueDate = now.AddDays(14);
            BorrowExtensions = new HashSet<BorrowExtension>();
        }

        public bool IsOverdue()
        {
            return !IsReturned && DateTime.Now > DueDate;
        }

        public int DaysLeft()
        {
            if (IsReturned) return 0;
            var today = DateTime.Now.Date;
            return (DueDate.Date - today).Days;
        }

        public void Extend(int days = 7)
        {
            if (!IsReturned)
            {
                DueDate = DueDate.AddDays(days);
            }
        }
    }
}
