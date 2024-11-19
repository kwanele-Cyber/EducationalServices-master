using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EducationalServices.Models;
using Microsoft.AspNet.Identity;

namespace EducationalServices.Controllers
{
    public class ProductsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public ActionResult CancelOrder()
        {
            Session["cart"] = null;
            TempData["Canceled"] = "Your Order Has Been Cancelled";
            return RedirectToAction("Index");
        }
        public ActionResult OrderDetails(int OrderId)
        {
            return View(db.OrderDetails.Where(x => x.OrderId == OrderId).ToList());
        }
        public ActionResult TrackOrder(int? id)
        {
            var order = db.Orders.Find(id);
            return View(order);
        }

        public ActionResult Orders()
        {
            var UserId = User.Identity.GetUserId();
            if (User.IsInRole("Admin"))
            {
                return View(db.Orders.ToList());
            }
            return View(db.Orders.Where(x=>x.UserId== UserId).ToList());
        }
        [HttpGet]
        public ActionResult Payment(int? CardId)
        {
            var Card =  db.Cards.Where(x=>x.CardId == CardId).FirstOrDefault();
            return View(Card);
        }
        [HttpPost]
        public ActionResult MakePayment()
        {
            var cart = Session["cart"] as List<Cart> ?? new List<Cart>();
            if (cart != null)
            {
                OrderDetails orderDetails = new OrderDetails();
                Orders orders = new Orders();
                decimal totalAmount = 0;
                orders.OrderDate = DateTime.Now;
                orders.UserId = User.Identity.GetUserId();
                foreach (var item in cart)
                {
                    totalAmount += item.Price * item.StockQuantity;
                }
                orders.TotalAmount = totalAmount + (totalAmount * 15 / 100);
                db.Orders.Add(orders);
                db.SaveChanges();
                foreach (var item in cart)
                {
                    var Product = db.Products.Find(item.ProductId);

                    orderDetails.OrderId = orders.OrderId;
                    orderDetails.ProductId = item.ProductId;
                    orderDetails.Name = item.Name;
                    orderDetails.Description = item.Description;
                    orderDetails.Price = item.Price;
                    orderDetails.StockQuantity = item.StockQuantity;
                    orderDetails.Weight = item.Weight;
                    orderDetails.Picture = item.Picture;

                    Product.StockQuantity -= item.StockQuantity;
                    db.OrderDetails.Add(orderDetails);
                    db.SaveChanges();
                }
                Session["cart"] = null;
                TempData["Success"] = "Your Order Has Place. Thank You For Your Time!";
                return RedirectToAction("Orders");
            }

            return RedirectToAction("ViewCart");
        }
        [HttpGet]
        public ActionResult ViewCart()
        {
            var cart = Session["cart"] as List<Cart> ?? new List<Cart>();
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int ProductId, int? Qty)
        {
            if (Qty == null || Qty <= 0)
            {
                Qty = 1;
            }

            var cart = Session["cart"] as List<Cart> ?? new List<Cart>();

            var GetProduct = db.Products.Find(ProductId);
            string UserId = User.Identity.GetUserId();
            var productInCart = cart.FirstOrDefault(x => x.ProductId == ProductId);

            if (productInCart == null && GetProduct.StockQuantity >= 1)
            {
                var newCartItem = new Cart()
                {
                    ProductId = ProductId,
                    Name = GetProduct.Name,
                    Description = GetProduct.Description,
                    Price = GetProduct.Price,
                    StockQuantity = (int)Qty,
                    Weight = GetProduct.Weight,
                    Picture = GetProduct.Picture,
                    UserId = UserId,
                };

                cart.Add(newCartItem);
            }
            else if (productInCart != null && (GetProduct.StockQuantity - productInCart.StockQuantity) >= 1)
            {
                
                productInCart.StockQuantity += (int)Qty; 
                productInCart.Price += (productInCart.StockQuantity * GetProduct.Price); 
            }

            Session["cart"] = cart;

            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        public ActionResult UpdateCart(int productId, int newQuantity)
        {
            if (productId <= 0 || newQuantity <= 0)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var cart = Session["cart"] as List<Cart> ?? new List<Cart>();

            var productInCart = cart.FirstOrDefault(x => x.ProductId == productId);
            if (productInCart == null)
            {
                return Json(new { success = false, message = "Product not found in the cart." });
            }

            productInCart.StockQuantity = newQuantity;

            Session["cart"] = cart;


            decimal cartTotal = CalculateCartTotal(cart);

            return Json(new { success = true, cartTotal = cartTotal, message = "Cart updated successfully." });
        }

        // Helper function to calculate the cart total
        private decimal CalculateCartTotal(List<Cart> cart)
        {
            decimal total = 0;
            foreach (var item in cart)
            {
                total += item.Price * item.StockQuantity;
            }
            return total;
        }







        public ActionResult DeleteCartItem(int ProductId)
        {
            var cart = Session["cart"] as List<Cart> ?? new List<Cart>();

            var itemToRemove = cart.FirstOrDefault(x => x.ProductId == ProductId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                Session["cart"] = cart;
            }

            return RedirectToAction("ViewCart");
        }
        // GET: Products
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,Name,Description,Price,StockQuantity,Weight,CreatedAt")] Product product, HttpPostedFileBase Picture)
        {
            if (ModelState.IsValid)
            {
                if (Picture != null && Picture.ContentLength > 0)
                {
                    using (var reader = new BinaryReader(Picture.InputStream))
                    {
                        var imageData = reader.ReadBytes(Picture.ContentLength);
                        product.Picture = Convert.ToBase64String(imageData);
                    }
                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Description,Price,StockQuantity,Weight,CreatedAt")] Product product, HttpPostedFileBase Picture)
        {
            if (ModelState.IsValid)
            {
                if (Picture != null && Picture.ContentLength > 0)
                {
                    using (var reader = new BinaryReader(Picture.InputStream))
                    {
                        var imageData = reader.ReadBytes(Picture.ContentLength);
                        product.Picture = Convert.ToBase64String(imageData);
                    }
                }
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
