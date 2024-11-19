using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalServices.Models
{
    public class BookIssueReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int BorrowId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string IssueType { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime ReportDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Pending, Under Review, Resolved

        [Range(0, 999999.99)]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal? AssessedFee { get; set; }

        [StringLength(1000)]
        public string AdminComments { get; set; }

        public DateTime? ResolutionDate { get; set; }

        // Navigation properties
        public virtual Borrow Borrow { get; set; }
        public virtual Book Book { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}