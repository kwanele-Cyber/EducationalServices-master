using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class QuizResultsViewModel
    {
        public Quiz Quiz { get; set; }

        public List<QuizAttempt> StudentsWhoCompleted { get; set; }

        public List<QuizAttempt> StudentsStillTaking { get; set; }

        public List<ApplicationUser> StudentsNotAttempted { get; set; }

        public bool IsAdminOrTutor { get; set; }
    }

}