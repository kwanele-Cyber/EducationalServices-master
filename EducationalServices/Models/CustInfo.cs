using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class CustInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string RecipientName { get; set; }

        public string RecipientNumber { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime deliveryDate { get; set; }

        public string preffaredTime { get; set; }

        public string ShippingMethod { get; set; }

        public double deliveryFee { get; set; }
    }
}