using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class SupportRequest
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public virtual ApplicationUser Student { get; set; }

        public string Issue { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        public string Resolution { get; set; }
    }

}