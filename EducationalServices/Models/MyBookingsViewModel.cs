using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationalServices.Models
{
    public class MyBookingsViewModel
    {
        public int RoomBookingId { get; set; }
        public string RoomName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string Base64Img { get; set; }
        public string RemainingTime { get; set; }
    }
}
