using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class RecommendationViewModel
    {
        public ApplicationUser Student { get; set; }
        public List<StudentModule> Modules { get; set; }
    }
}