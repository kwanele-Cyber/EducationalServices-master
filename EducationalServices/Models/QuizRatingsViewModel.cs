using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizRatingsViewModel
    {
        public Quiz Quiz { get; set; }

        public List<QuizRating> Ratings { get; set; }
    }
}