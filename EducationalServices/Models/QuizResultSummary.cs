using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizResultSummary
    {
        public Quiz Quiz { get; set; }

        public int TotalAttempts { get; set; }

        public int CompletedAttempts { get; set; }

        public double? AverageScore { get; set; }

        // New properties
        public int? HighestScore { get; set; }

        public int? LowestScore { get; set; }

        public double? PassRate { get; set; }
    }
}
