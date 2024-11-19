using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class BookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImagePath { get; set; }
        public BookStatus Status { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public bool IsReservedByUser { get; set; }
    }

}