using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class RoomScheduleViewModel
    {
        public Room Room { get; set; }
        public List<RoomBooking> Bookings { get; set; }
        public DateTime CurrentDate { get; set; }
    }

}