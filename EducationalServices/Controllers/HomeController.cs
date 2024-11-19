﻿// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.HomeController
using System.Web.Mvc;

public class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }

    public ActionResult About()
    {
        base.ViewBag.Message = "Your application description page.";
        return View();
    }

    public ActionResult Contact()
    {
        base.ViewBag.Message = "Your contact page.";
        return View();
    }
}
