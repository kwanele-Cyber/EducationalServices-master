using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class RoomBooking
    {
        public int RoomBookingId { get; set; }
        public int RoomId { get; set; }
        public string VerificationId { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public bool IsActive { get; set; }

        public virtual Room Room { get; set; }
        public virtual ApplicationUser User { get; set; }
        [ForeignKey(nameof(VerificationId))]
        public virtual VerificationCode VerificationCode {  get; set; }
    }

}