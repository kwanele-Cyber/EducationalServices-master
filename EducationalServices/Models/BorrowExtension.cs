using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalServices.Models
{
    public class BorrowExtension
    {
        [Key]
        public int BorrowExtensionId { get; set; }

        [Required]
        public int BorrowId { get; set; }

        [Required]
        public DateTime ExtensionDate { get; set; }

        [Required]
        public int DaysExtended { get; set; }

        [Required]
        public string UserId { get; set; }

        // Navigation properties
        [ForeignKey("BorrowId")]
        public virtual Borrow Borrow { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public BorrowExtension()
        {
            ExtensionDate = DateTime.Now;
            DaysExtended = 7; // Default extension period
        }
    }
}
