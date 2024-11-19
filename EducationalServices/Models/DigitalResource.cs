using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalServices.Models
{
    public class DigitalResource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Author { get; set; }

        public string Type { get; set; } // e.g., "Journal", "E-Book", "Article"

        public string FileUrl { get; set; }

        public string Description { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
