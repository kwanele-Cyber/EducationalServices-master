using System.ComponentModel.DataAnnotations;
using System;



namespace EducationalServices.Models
{
    public class BorrowViewModel
    {
        public string QRCode { get; set; }

        public int BookId { get; set; }
        public string BookTitle { get; set; }

        [Required]
        [Display(Name = "Expected Return Date")]
        [DataType(DataType.Date)]
        public DateTime ExpectedReturnDate { get; set; }
        public string ScannedCode { get; set; }
        public string UserId { get; set; }
    }
}