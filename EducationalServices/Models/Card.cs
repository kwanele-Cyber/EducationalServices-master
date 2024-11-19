using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class Card
    {
        [Key]
        public int CardId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public string ExpirationDate { get; set; }
        public string UserId { get; set; }
    }
}