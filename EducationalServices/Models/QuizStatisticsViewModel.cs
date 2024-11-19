using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{

    public class QuizStatisticsViewModel
    {
        public Quiz Quiz { get; set; }

        public int TotalAttempts { get; set; }

        public double AverageScore { get; set; }

        public int HighestScore { get; set; }

        public int LowestScore { get; set; }

        public int CompletedAttempts { get; set; }

        public int IncompleteAttempts { get; set; }

        public float CompletionRate { get; set; }

        public List<QuestionStatistics> QuestionStatistics { get; set; }
    }

}