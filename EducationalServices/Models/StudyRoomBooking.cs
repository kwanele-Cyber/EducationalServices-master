using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class StudyRoomBooking
    {
        public int StudyRoomBookingId { get; set; }
        public string UserId { get; set; }  // Tracks the user who booked the study room
        public DateTime StartTime { get; set; }
        public int DurationInHours { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }

}