using System;
using System.Web;
using static EducationalServices.Models.BookInspection;

namespace EducationalServices.Models
{
    public partial class BookInspectionViewModel
    {
        public int BookId { get; set; }

        public string UserId { get; set; }
        public int BorrowId { get; set; }

        public BookInspectionStatus Status { get; set; }

        public string Notes { get; set; }

    }
}