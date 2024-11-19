using System;
using System.ComponentModel.DataAnnotations;



namespace EducationalServices.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public string Weight { get; set; }

        public string Picture { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}