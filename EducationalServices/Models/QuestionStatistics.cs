using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuestionStatistics
    {
        public string QuestionText { get; set; }

        public int CorrectAnswers { get; set; }

        public int IncorrectAnswers { get; set; }

        public float CorrectPercentage { get; set; }
    }

}