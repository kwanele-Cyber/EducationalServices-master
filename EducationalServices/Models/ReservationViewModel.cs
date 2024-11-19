using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class ReservationViewModel
    {
        public int ReservationId { get; set; }
        public string UserId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsBookAvailable { get; set; }
        public ReservationStatus Status { get; set; }
        public string QRCode { get; set; }
    }

}


namespace EducationalServices.Models
{
    public class ReserveBookVM
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string ImagePath { get; set; }

        [DisplayName("PickUp Date")]
        public DateTime ReservationStart { get; set; } = DateTime.UtcNow;
    }
}