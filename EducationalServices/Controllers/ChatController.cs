// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.ChatController
using System.Web.Mvc;

[Authorize]
public class ChatController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
