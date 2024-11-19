using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EducationalServices.Controllers
{
    public class CourseRecommendationsController : Controller
    {
        // GET: CourseRecommendations
        public ActionResult Index()
        {
            return View();
        }
    }
}