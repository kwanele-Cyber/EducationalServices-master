using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    // EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
    // EducationalServices.Models.Cart
    public class Cart
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Weight { get; set; }

        public string Picture { get; set; }

        public string UserId { get; set; }
    }

}