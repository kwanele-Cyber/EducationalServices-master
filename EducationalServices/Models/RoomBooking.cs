using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class RoomBooking
    {
        public int RoomBookingId { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }

        public virtual Room Room { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}