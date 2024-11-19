// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.PayPalController
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using EducationalServices;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;
using PayPal.Api;

public class PayPalController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult CreatePayment(double CartTotal, int? modID, bool isRecommendation = false)
    {
        if (modID.HasValue)
        {
            base.Session["ModID"] = modID.ToString();
            base.Session["IsRecommendation"] = isRecommendation.ToString();
        }
        else
        {
            base.Session["ModID"] = "0";
            base.Session["IsRecommendation"] = "false";
        }
        string CurrentUser = base.User.Identity.Name;
        double convertedTot = Math.Round(CartTotal / 14.357);
        int Rem = (int)(CartTotal % 14.357);
        string Cost = convertedTot + "." + Rem;
        APIContext apiContext = PayPalConfig.GetAPIContext();
        string clientId = ConfigurationManager.AppSettings["PayPalClientId"];
        string clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
        apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
        string accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
        apiContext.AccessToken = accessToken;
        Payment payment = new Payment
        {
            intent = "sale",
            payer = new Payer
            {
                payment_method = "paypal"
            },
            transactions = new List<Transaction>
            {
                new Transaction
                {
                    amount = new Amount
                    {
                        total = Cost,
                        currency = "USD"
                    },
                    description = "Module Payment"
                }
            },
            redirect_urls = new RedirectUrls
            {
                return_url = base.Url.Action("CompletePayment", "PayPal", null, base.Request.Url.Scheme),
                cancel_url = base.Url.Action("CancelPayment", "PayPal", null, base.Request.Url.Scheme)
            }
        };
        Payment createdPayment = payment.Create(apiContext);
        string approvalUrl = createdPayment.links.FirstOrDefault((Links l) => l.rel == "approval_url")?.href;
        return Redirect(approvalUrl);
    }

    public ActionResult CompletePayment(string paymentId, string token, string PayerID)
    {
        APIContext apiContext = PayPalConfig.GetAPIContext();
        PaymentExecution paymentExecution = new PaymentExecution
        {
            payer_id = PayerID
        };
        Payment executedPayment = new Payment
        {
            id = paymentId
        }.Execute(apiContext, paymentExecution);
        return RedirectToAction("PaymentSuccess", new
        {
            PayID = paymentId
        });
    }

    public ActionResult CancelPayment()
    {
        base.TempData["Error"] = "Payment was cancelled.";
        return RedirectToAction("RegisterForModule", "Modules");
    }

    public ActionResult PaymentSuccess(string PayID)
    {
        if (base.Session["ModID"] != null && base.Session["IsRecommendation"] != null)
        {
            string modID = base.Session["ModID"] as string;
            bool isRecommendation = bool.Parse(base.Session["IsRecommendation"] as string);
            if (modID != "0")
            {
                int ModuleId = int.Parse(modID);
                Module module = db.Modules.Find(ModuleId);
                if (module == null)
                {
                    base.TempData["Error"] = "Module not found.";
                    return RedirectToAction("Index", "Home");
                }
                string userId = base.User.Identity.GetUserId();
                StudentModule existingEnrollment = db.StudentModules.FirstOrDefault((StudentModule sm) => sm.StudentId == userId && sm.ModuleId == ModuleId);
                if (existingEnrollment != null)
                {
                    base.TempData["Info"] = "You are already enrolled in this module.";
                    return RedirectToAction("RegisteredStudent", "Modules");
                }
                StudentModule studentModule = new StudentModule
                {
                    ModuleId = ModuleId,
                    StudentId = userId,
                    Status = "Paid",
                    TotalPaid = module.Price
                };
                db.StudentModules.Add(studentModule);
                if (isRecommendation)
                {
                    CourseRecommendation recommendation = db.CourseRecommendations.FirstOrDefault((CourseRecommendation cr) => cr.StudentId == userId && cr.ModuleId == ModuleId);
                    if (recommendation != null)
                    {
                        db.CourseRecommendations.Remove(recommendation);
                    }
                }
                db.SaveChanges();
                base.TempData["Success"] = "Your enrollment has been completed. Thank you for your payment!";
                return RedirectToAction("RegisteredStudent", "Modules");
            }
        }
        base.TempData["Error"] = "An error occurred during the payment process.";
        return RedirectToAction("Index", "Home");
    }
}
