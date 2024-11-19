using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizRatingDisplayViewModel
    {
        public int RatingId { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string StudentName { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime DateRated { get; set; }

        public int Score { get; set; }
    }

}