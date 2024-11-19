using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizAttemptAnswerViewModel
    {
        public int AnswerId { get; set; }

        public string QuestionText { get; set; }

        public string StudentAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public int Marks { get; set; }

        public int MarksObtained { get; set; }
    }

}