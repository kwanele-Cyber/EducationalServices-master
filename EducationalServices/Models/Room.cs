using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; }

        public List<RoomBooking> RoomBookings {  get; set; }
    }

}