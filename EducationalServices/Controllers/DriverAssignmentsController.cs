using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

using EducationalServices.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using static iTextSharp.text.TabStop;
using Org.BouncyCastle.Asn1.X509;
using System.Deployment.Internal;


namespace EducationalServices.Controllers
{
    public class DriverAssignmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DriverAssignments
        public ActionResult Index()
        {
            var driverAssignments = db.DriverAssignments.Include(d => d.Driver).Include(d => d.Orders);
            return View(driverAssignments.ToList());
        }
        public ActionResult MyAssignments(string email = "Email")
        {
            if (email == "Email")
            {
                email = User.Identity.Name;
                ViewBag.Title = "Assignments";
            }
            else
            {
                ViewBag.Title = email+"'s Assignments";
            }
            var driverAssignments = db.DriverAssignments.Include(d => d.Driver).Include(d => d.Orders).Where(x=>x.Driver.Email == email);
            return View(driverAssignments.ToList());
        }


            public ActionResult OrderReady(int id)
        {
            var assignment = db.DriverAssignments.Find(id);
            var order = db.Orders.Find(assignment.OrderId);
            assignment.Status = "Ready for pickup";
            order.Status = "Ready for pickup";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(assignment.Email);
                email2.Subject = "Order Ready |  " + assignment.OrderId;
                string emailBody = $"Order Number: " + assignment.OrderId + "\t\tDelivery Date: " + assignment.DeliveryDate + "\t\t Delivery Time: " + assignment.DeliveryTime + " \n\n" +
               $"Hi {assignment.Driver.Name}, \n\n" +
               $"Please note that, order {assignment.OrderId} is ready to be picked up for delivery to address {assignment.Orders.Address}, please procced with order delivery within due time.\n\n" +



               $"Regards,\r\nEducational Services";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("Index");
            }
            db.SaveChanges();
            TempData["Message"] = "Order Successfully Marked as Ready for Delivery";
            return RedirectToAction("Index");
        }

        public ActionResult DispatchOrder(int id)
        {
            var ass = db.DriverAssignments.Where(x=>x.OrderId == id).FirstOrDefault();
            var order = db.Orders.Find(ass.OrderId);
            order.Status = "PikingUp";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            DispatchRecord b = new DispatchRecord()
            {
                OrderNumber = id,
                AssDrivId = ass.AssDrivId,
                DriverName = ass.Driver.Name,
                DriverSurnane = ass.Driver.Surname

            };
            return View(b);
        }
        [HttpPost]
        public ActionResult DispatchOrder(DispatchRecord dispatchRecords)
        {
            string base64Signature = dispatchRecords.Signature;

            string signatureImagePath = SaveSignatureImage(base64Signature);
            string folderPath = Server.MapPath("~/App_Data/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileName = $"Document-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);
            CreatePdfWithSignature(signatureImagePath, filePath, dispatchRecords.AssDrivId);


            var ass = db.DriverAssignments.Find(dispatchRecords.AssDrivId);
            var order = db.Orders.Find(ass.OrderId);
            ass.Status = "PickedUp";
            order.Status = "PickedUp";
            order.PickupDate = DateTime.Now;
            string recipientEmail = ass.Driver.Email;
            db.Entry(ass).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            SendPdfEmail(recipientEmail, filePath);


            dispatchRecords.filePath = filePath;
            db.DispatchRecords.Add(dispatchRecords);
        

            db.SaveChanges();
            return View(dispatchRecords);
        }
        private void CreatePdfWithSignature(string signatureImagePath, string filePath, int id)
        {
            var ass = db.DriverAssignments.Find(id);
            
            
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, fileStream);
                document.Open();

                
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                Font subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.DARK_GRAY);
                Font bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

                
                Paragraph title = new Paragraph("Dispatch Order Record", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);

                // Add subtitle
                Paragraph subtitle = new Paragraph("Dispatch Information", subtitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(subtitle);

                // Add document content
                Paragraph content1 = new Paragraph("Order Number: " + ass.OrderId, bodyFont)
                {
                    SpacingAfter = 10
                };
                Paragraph content2 = new Paragraph("Driver Name: " + ass.Driver.Name, bodyFont)
                {
                    SpacingAfter = 10
                };
                Paragraph content3 = new Paragraph("Driver Surname: " + ass.Driver.Surname, bodyFont)
                {
                    SpacingAfter = 20
                };
               
                document.Add(content1);
                document.Add(content2);
                document.Add(content3);

                Paragraph subtitle2 = new Paragraph("Delivery Information", subtitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(subtitle);

                Paragraph content4= new Paragraph("Delivery Date: " + ass.DeliveryDate, bodyFont)
                {
                    SpacingAfter = 10
                };
                Paragraph content5 = new Paragraph("Delivery Time: " + ass.DeliveryTime, bodyFont)
                {
                    SpacingAfter = 20
                };
                Paragraph content6 = new Paragraph("Date: " + DateTime.Now.ToShortDateString(), bodyFont)
                {
                    SpacingAfter = 10
                };
                document.Add(content4);
                document.Add(content5);
                document.Add(content6);


                // Add Signature Section as an image
                document.Add(new Paragraph("\n\nSignature:", bodyFont));

                try
                {
                    if (System.IO.File.Exists(signatureImagePath))
                    {
                        // Add signature image if it exists
                        Image signatureImage = Image.GetInstance(signatureImagePath);
                        signatureImage.ScaleToFit(200f, 80f); // Adjust the size as needed
                        signatureImage.Alignment = Element.ALIGN_LEFT; // Align as needed
                        document.Add(signatureImage);
                    }
                    else
                    {
                        document.Add(new Paragraph("Signature image not found.", bodyFont));
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions related to image processing
                    document.Add(new Paragraph($"Error adding signature image: {ex.Message}", bodyFont));
                }

                // Finalize the document
                document.Close();
            }
        }
        private string SaveSignatureImage(string base64Signature)
        {
            // Decode base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(base64Signature);

            // Application folder path where signature images will be saved
            string folderPath = Server.MapPath("~/App_Data/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create file path for the signature image
            string imagePath = Path.Combine(folderPath, $"Signature-{DateTime.Now:yyyyMMddHHmmss}.png");

            // Save the byte array to the image file
            System.IO.File.WriteAllBytes(imagePath, imageBytes);

            return imagePath;
        }
        private void SendPdfEmail(string recipient, string pdfFilePath)
        {
            string senderEmail = "youraddress@example.com";
            string senderPassword = "yourpassword";

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Dispatch PDF Document",
                Body = "Please find the attached document for order dispatch."
            };

            // Add recipient
            mail.To.Add(recipient);

            // Attach the PDF from the saved path
            mail.Attachments.Add(new Attachment(pdfFilePath));

            // Configure SMTP client
            SmtpClient smtp = new SmtpClient("smtp.example.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            // Send email
            smtp.Send(mail);
        }





        [HttpPost]
    public ActionResult StartOrderDelivery(int id, string estArrivalTime)
        {
            var assignment = db.DriverAssignments.Find(id);
            assignment.DeliveryTime = estArrivalTime;
            var order = db.Orders.Find(assignment.OrderId);
            assignment.Status = "On the way";
            order.Status = "On the way";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Order Delivery Started | " + assignment.OrderId;
                string emailBody = $"Order Number: " + assignment.OrderId + "\t\t Estimated Arrival Time: " + estArrivalTime + " \n\n" +
                    $"Hi {assignment.Driver.Name}, \n\n" +
                    $"Please note that, order {assignment.OrderId} is picked up by friver for delivery to address {order.Address}.\n\n" +
                    $"Your order is now out for delivery. The driver will be at your door around {estArrivalTime}.\n\n" +
                    $"Please present this unique code to the driver: {order.Code}\n\n or the attached QR Code." +

                    $"Regards,\r\nEducational Services";
                email2.Body = emailBody;

                
                
               
                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyAssignments");
            }
            db.SaveChanges();
            TempData["Success"] = "Order Delivery Sucessfully Started Please Navigate to Customer Location";
            return RedirectToAction("Navigation", new { id = order.OrderId });

        }
        public ActionResult Navigation(int id)
        {
            HttpCookie cookie = new HttpCookie("OrdID");
            // Set cookie value
            cookie.Value = id.ToString();

            // Set cookie expiration (optional)
            cookie.Expires = DateTime.Now.AddDays(1);
            // Add the cookie to the response
            Response.Cookies.Add(cookie);

            var order = db.Orders.Find(id);
            ViewBag.DestinationAddress = order.Address;
            return View();
        }

        public ActionResult FinishOrderDelivery()
        {
            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var order = db.Orders.Find(id);
                order.Status = "Driver Waiting";
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Something went wrong";
                return RedirectToAction("MyAssignments");
            }
            

            return View();
        }
        [HttpPost]
        public ActionResult FinishOrderDelivery(string code)
        {


            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var order = db.Orders.Find(id);

                var assign = db.DriverAssignments.Where(x => x.OrderId == id && x.Email == User.Identity.Name).FirstOrDefault();
                var userData = db.CustInfos.Where(x => x.Email == order.Email).FirstOrDefault();
                db.Entry(order).State = EntityState.Modified;
                db.Entry(assign).State = EntityState.Modified;
                if (int.Parse(code) == order.Code)
                {
                    order.Status = "Order Recived";
                    order.DeliveredOn = DateTime.Now;
                    order.DeliveredBy = assign.DrivId;
                    assign.Status = "Completed";

                    assign.IsActive = false;
                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("Poultry.dbn@outlook.com");
                        email2.To.Add(order.Email);
                        email2.Subject = "Order Delivered";
                        string emailBody = $"Order Number: " + order.OrderId + " \n\n" +
                       $"Dear {userData.Name}, \n\n" +
                       $"Your order {order.OrderId} has been delivered to {order.Address} on {DateTime.Now.Date.ToLongDateString()} at {DateTime.Now.ToShortTimeString()}.\n\n" +

                       $"Thank you for shopping with us!\n\n" +
                       $"Regards,\r\nEducational Services";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("FinishOrderDelivery");
                    }

                    db.SaveChanges();

                }
                else
                {
                    TempData["Error"] = "Incorrect Code, Please Try again";
                    return RedirectToAction("FinishOrderDelivery");
                }
                cookie.Expires = DateTime.Now.AddDays(-1); 
                Response.Cookies.Add(cookie);
                TempData["Success"] = "Order Delivered Sucessfully";
                return RedirectToAction("MyAssignments");
            }




            else
            {
                TempData["Error"] = "Something went wrong";
                return RedirectToAction("MyAssignments");
            }
        }

        [HttpPost]
        public ActionResult NoResponseAction()
        {
            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var order = db.Orders.Find(id);
                var userData = db.CustInfos.Where(x => x.Email == order.Email).FirstOrDefault();

                var assign = db.DriverAssignments.Where(x => x.OrderId == id).FirstOrDefault();
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;
          
                    order.Status = "No Response";
                    assign.Status = "No Response";
                    assign.IsActive = false;
                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("Poultry.dbn@outlook.com");
                        email2.To.Add(order.Email);
                        email2.Subject = "No Response";
                    string emailBody;
                    if (!order.IsDeliveryRescheduled)
                    {
                        emailBody = $"Order Number: " + order.OrderId + " \n\n" +
                       $"Dear {userData.Name}, \n\n" +
                       $"Your order {order.OrderId} could not be delivered, driver tried to reach you at {order.Address} on {DateTime.Now.Date.ToLongDateString()} around {DateTime.Now.ToShortTimeString()} but there was no one to recieve order.\n\n" +
                       $"Your delivery will be rescheduled for another day. \n\n " +
                       $"Failure to collect order on the next date will result in cancellation and your order amount will be refunded to your account. \n\n " +
                       $"Thank you for shopping with us!\n\n" +
                       $"Regards,\r\nEducational Services";
                    }
                    else
                    {

                    
                       emailBody = $"Order Number: " + order.OrderId + " \n\n" +
                       $"Dear {userData.Name}, \n\n" +
                       $"Your order {order.OrderId} could not be delivered, driver tried to reach you at {order.Address} on {DateTime.Now.Date.ToLongDateString()} around {DateTime.Now.ToShortTimeString()} but there was no one to recieve order.\n\n" +
                       $"Your order amount of {order.TotalAmount} will be reversed to your account. \n\n " +
                       $"Failure to collect order on the next date will result in cancellation and your order amount will be refunded to your account. \n\n " +
                       $"Thank you for shopping with us!\n\n" +
                       $"Regards,\r\nEducational Services";
                    }
                    email2.Body = emailBody;
                   

                    var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("FinishOrderDelivery");
                    }

                    db.SaveChanges();
                
                




            }
            else
            {
                TempData["Error"] = "Something went Wrong Please try again later";
                return RedirectToAction("FinishOrderDelivery");
            }
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            TempData["Error"] = "Customer did not arrive on time, please proceed with other deliveries.";
            return RedirectToAction("MyAssignments");
        }



        // GET: DriverAssignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Create
        public ActionResult Create(int id)
        {
            var Driv = db.Drivers.Find(id);
            string OrdID;
            if (Session["CustOrdID"] != null)
            {
                OrdID = Session["CustOrdID"] as string;
            }
            else
            {
                return RedirectToAction("Orders", "Products");
            }
            int ordId = int.Parse(OrdID);
            var order = db.Orders.Find(ordId);
            DriverAssignment b = new DriverAssignment()
            {
                OrderId = ordId,
                DrivId = id,
                Email = Driv.Email,
                GenDeliveryDate = order.deliveryDate.ToLongDateString(),

            };
            

            
            return View(b);
        }

        // POST: DriverAssignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssDrivId,OrderId,DrivId,Name,Surname,Email,DeliveryDate,DeliveryTime,GenDeliveryDate,preffaredTime")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                var order = db.Orders.Find(driverAssignment.OrderId);
                var userdata = db.CustInfos.Where(x => x.Email == order.Email).FirstOrDefault();
                var assign = db.DriverAssignments.Where(x => x.OrderId == driverAssignment.OrderId && x.DrivId == driverAssignment.DrivId).FirstOrDefault();
                if (assign == null)
                {
                    var assign2 = db.DriverAssignments.Where(x => x.OrderId == driverAssignment.OrderId).FirstOrDefault();
                    if (assign2 == null)
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                       
                        order.Status = "Delivery Scheduled";
                        order.DriverEmail = driver.Email;
                        driverAssignment.Surname = driver.Surname;
                        driverAssignment.Name = driver.Name;
                        driverAssignment.Status = "Assigned";
                        driverAssignment.Created = DateTime.Now;
                        driverAssignment.IsActive = true;
                        db.Entry(order).State = EntityState.Modified;
                        db.DriverAssignments.Add(driverAssignment);
                        try
                        {
                            // Prepare email message
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(User.Identity.Name);
                            email.Subject = "Delivery Assignment |  " + order.OrderId;
                            string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {driver.Name}, \n\n" +
                           $"Please note that, you have been assigned to a new delivery for order {order.OrderId} to {order.Address}.\n\n" +


                           $"We'll email you the moment the order is ready for pickup." +
                           $"Regards,\r\nEducational Services";
                            email.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email);



                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(User.Identity.Name);
                            email2.Subject = "Delivery Scheduled |  " + order.OrderId;
                            string emailBody2 = $"Order Number: " + order.OrderId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {userdata.Name}, \n\n" +
                           $"Please note that, we’ve scheduled delivery for order {order.OrderId}\n\n" +
                           $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                           $"We'll email you the moment the delivery starts." +
                           $"Regards,\r\nEducational Services";
                            email2.Body = emailBody2;




                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "OrderedProducts");
                        }
                        db.SaveChanges();

                        
                        Session["CustOrdID"] = null;

                        TempData["Success"] = "Driver Assigned Sucessfully";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        
                        order.Status = "Delivery Scheduled";
                        order.DriverEmail = driver.Email;
                        order.DeliveredBy = driverAssignment.DrivId;
                        order.IsDeliveryRescheduled = true;

                        driverAssignment.Surname = driver.Surname;
                        driverAssignment.Name = driver.Name;
                        driverAssignment.Status = "Assigned";
                        driverAssignment.Created = DateTime.Now;
                        driverAssignment.IsActive = true;
                        db.Entry(order).State = EntityState.Modified;
                        db.DriverAssignments.Add(driverAssignment);
                        try
                        {
                            // Prepare email message
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(User.Identity.Name);
                            email.Subject = "Delivery Assignment |  " + order.OrderId;
                            string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {driver.Name}, \n\n" +
                           $"Please note that, you have been assigned to a new delivery for order {order.OrderId} to {order.Address}.\n\n" +


                           $"We'll email you the moment the order is ready for pickup." +
                           $"Regards,\r\nEducational Services";
                            email.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email);

                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(User.Identity.Name);
                            email2.Subject = "Delivery Rescheduled |  " + order.OrderId;
                            string emailBody2 = $"Order Number: " + order.OrderId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {userdata.Name}, \n\n" +
                           $"Please note that, we’ve rescheduled delivery for order {order.OrderId}\n\n" +
                           $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                           $"We'll email you the moment the delivery starts." +
                           $"Regards,\r\nEducational Services";
                            email2.Body = emailBody2;




                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "OrderedProducts");
                        }
                        db.SaveChanges();

                        Session["CustOrdID"] = null;
                        

                        TempData["Success"] = "Driver Assigned Sucessfully";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    var driver = db.Drivers.Find(driverAssignment.DrivId);
                   
                    order.Status = "Delivery Scheduled";
                    order.DriverEmail = driver.Email;
                    order.IsDeliveryRescheduled = true;
                    assign.Status = "Assigned";
                    assign.Created = DateTime.Now;
                    assign.IsActive = true;
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;

                    try
                    {
                        // Prepare email message
                        var email = new MailMessage();
                        email.From = new MailAddress("Poultry.dbn@outlook.com");
                        email.To.Add(User.Identity.Name);
                        email.Subject = "Delivery Assignment |  " + order.OrderId;
                        string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                       $"Hi {driver.Name}, \n\n" +
                       $"Please note that, you have been assigned to a new delivery for order {order.OrderId} to {order.Address}.\n\n" +


                       $"We'll email you the moment the order is ready for pickup." +
                       $"Regards,\r\nEducational Services";
                        email.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email);


                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("Poultry.dbn@outlook.com");
                        email2.To.Add(User.Identity.Name);
                        email2.Subject = "Delivery Rescheduled |  " + order.OrderId;
                        string emailBody2 = $"Order Number: " + order.OrderId + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                       $"Hi {userdata.Name}, \n\n" +
                       $"Please note that, we’ve rescheduled delivery for order {order.OrderId}\n\n" +
                       $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                       $"We'll email you the moment the delivery starts." +
                       $"Regards,\r\nEducational Services";
                        email2.Body = emailBody2;




                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("Index", "OrderedProducts");
                    }
                    db.SaveChanges();

                    Session["ProductId"] = null;
                    Session["CustOrdID"] = null;

                    TempData["Success"] = "Driver Assigned Sucessfully";
                    return RedirectToAction("Index");
                }
            }

        

       
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "UserId", driverAssignment.OrderId);
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssDrivId,OrderId,DrivId,Name,Surname,Email,Status,DeliveryDate,DeliveryTime,Created,IsActive,GenDeliveryDate,preffaredTime")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(driverAssignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "UserId", driverAssignment.OrderId);
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            db.DriverAssignments.Remove(driverAssignment);
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
