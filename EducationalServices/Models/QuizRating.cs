using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizRating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int QuizId { get; set; }

        public virtual Quiz Quiz { get; set; }

        [Required]
        public string StudentId { get; set; }

        public virtual ApplicationUser Student { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }

        public virtual QuizAttempt QuizAttempt { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime DateRated { get; set; }
    }



}