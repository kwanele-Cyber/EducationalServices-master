using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizReviewQuestionViewModel
    {
        public int QuestionNumber { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string Options { get; set; }

        public string UserAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public string Explanation { get; set; }
    }

}