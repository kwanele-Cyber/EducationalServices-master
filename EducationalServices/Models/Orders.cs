using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime deliveryDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DeliveredOn { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime PickupDate { get; set; }

        public string Status { get; set; }

        public int Code { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal ProductCost { get; set; }

        public decimal TotalAmount { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string DriverEmail { get; set; }

        public int DeliveredBy { get; set; }

        public bool IsDeliveryRescheduled { get; set; }

        public string PaymentID { get; set; }
    }
}