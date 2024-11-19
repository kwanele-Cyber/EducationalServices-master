using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizReviewViewModel
    {
        public string QuizTitle { get; set; }

        public List<QuizReviewQuestionViewModel> Questions { get; set; }
    }
}