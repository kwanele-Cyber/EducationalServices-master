using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Owin;
using Owin;
using EducationalServices.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;

[assembly: OwinStartup(typeof(EducationalServices.Startup))]

namespace EducationalServices
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Call the seeding method
            SeedOpenSourceDigitalResources();
        }

        private void SeedOpenSourceDigitalResources()
        {
            using (var db = new ApplicationDbContext())
            {
                if (db.DigitalResources.Any())
                {
                    return; // DB has already been seeded
                }

                var resources = new List<DigitalResource>
                {
                    new DigitalResource
                    {
                        Title = "Introduction to Computer Science",
                        Author = "Harvard University",
                        Type = "Course",
                        FileUrl = "https://cs50.harvard.edu/x/2023/",
                        Description = "Harvard University's introduction to the intellectual enterprises of computer science and the art of programming.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "Project Gutenberg",
                        Author = "Various",
                        Type = "E-Book Library",
                        FileUrl = "https://www.gutenberg.org/",
                        Description = "A library of over 60,000 free eBooks, choose among free epub and Kindle eBooks, download them or read them online.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "Introduction to Statistical Learning",
                        Author = "Gareth James, Daniela Witten, Trevor Hastie, Robert Tibshirani",
                        Type = "Textbook",
                        FileUrl = "https://www.statlearning.com/",
                        Description = "This book provides an introduction to statistical learning methods. It is aimed for upper level undergraduate students, masters students and Ph.D. students in the non-mathematical sciences.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "Nature Journal",
                        Author = "Nature Publishing Group",
                        Type = "Scientific Journal",
                        FileUrl = "https://www.nature.com/nature/volumes",
                        Description = "Nature is a weekly international journal publishing the finest peer-reviewed research in all fields of science and technology.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "MIT OpenCourseWare",
                        Author = "Massachusetts Institute of Technology",
                        Type = "Course Materials",
                        FileUrl = "https://ocw.mit.edu/",
                        Description = "MIT OpenCourseWare is a web-based publication of virtually all MIT course content. OCW is open and available to the world and is a permanent MIT activity.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "arXiv",
                        Author = "Cornell University",
                        Type = "Preprint Repository",
                        FileUrl = "https://arxiv.org/",
                        Description = "arXiv is a free distribution service and an open-access archive for scholarly articles in the fields of physics, mathematics, computer science, quantitative biology, quantitative finance, statistics, electrical engineering and systems science, and economics.",
                        DateAdded = DateTime.Now
                    },
                    new DigitalResource
                    {
                        Title = "Khan Academy",
                        Author = "Sal Khan",
                        Type = "Educational Platform",
                        FileUrl = "https://www.khanacademy.org/",
                        Description = "Khan Academy offers practice exercises, instructional videos, and a personalized learning dashboard that empower learners to study at their own pace in and outside of the classroom.",
                        DateAdded = DateTime.Now
                    }
                };

                db.DigitalResources.AddRange(resources);
                db.SaveChanges();
            }
        }
    }
}
