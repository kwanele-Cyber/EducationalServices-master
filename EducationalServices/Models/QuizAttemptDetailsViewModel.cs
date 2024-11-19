using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizAttemptDetailsViewModel
    {
        public int AttemptId { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string StudentName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public double ScorePercentage { get; set; }

        public bool IsCompleted { get; set; }

        public List<QuizAttemptAnswerViewModel> Answers { get; set; }
    }
}