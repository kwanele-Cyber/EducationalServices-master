using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Feedback Type")]
        public FeedbackType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; }

        public bool IsResolved { get; set; }

        public string AdminResponse { get; set; }
    }

    public enum FeedbackType
    {
        Suggestion,
        Complaint
    }
}