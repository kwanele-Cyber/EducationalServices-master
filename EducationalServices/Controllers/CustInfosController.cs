// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Controllers.CustInfosController
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EducationalServices.Models;

public class CustInfosController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    [HttpPost]
    public ActionResult Update(string RecipientName, string RecipientNumber, string Address, string totalCost, DateTime deliveryDate, string preffaredTime, string ShippingMethod, string deliveryFee)
    {
        CustInfo custInfo = db.CustInfos.Where((CustInfo x) => x.Email == User.Identity.Name).FirstOrDefault();
        custInfo.RecipientName = RecipientName;
        custInfo.RecipientNumber = RecipientNumber;
        custInfo.Address = Address;
        custInfo.Email = base.User.Identity.Name;
        custInfo.deliveryDate = deliveryDate;
        custInfo.preffaredTime = preffaredTime;
        custInfo.ShippingMethod = ShippingMethod;
        string deleviveryFeeVal = deliveryFee.Substring(0, deliveryFee.Length - 3);
        custInfo.deliveryFee = double.Parse(deleviveryFeeVal);
        string totalCostVal = totalCost.Substring(0, totalCost.Length - 3);
        double FtotalCost = double.Parse(totalCostVal);
        db.Entry(custInfo).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("CreatePayment", "PayPal", new
        {
            CartTotal = FtotalCost,
            ShipMethod = ShippingMethod
        });
    }
}
