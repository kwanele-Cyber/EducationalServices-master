using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class BookOrder
    {
        [Key]
        public int BookOrderId { get; set; } // Changed from OrderId to BookOrderId for clarity

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        public decimal Price { get; set; }

        public DateTime OrderDate { get; set; }

        // Optionally, you can add a status field to track whether the order has been fulfilled or not
        public bool IsFulfilled { get; set; }
    }


}