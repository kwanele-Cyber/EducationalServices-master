using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class AvailableQuizzesViewModel
    {
        public List<Quiz> AvailableQuizzes { get; set; }
        public List<Quiz> PastQuizzes { get; set; }
    }

}