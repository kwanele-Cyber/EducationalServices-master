using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public Guid? VerificationCodeId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ReservationEndDate { get; set; }
        public int ReservationOrder { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.NONE;
        public virtual Book Book { get; set; }
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(VerificationCodeId))]
        public virtual VerificationCode VerificationCode { get; set; }
    }

}