using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public class DispatchRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DisID { get; set; }

        public int AssDrivId { get; set; }

        public int OrderNumber { get; set; }

        public string DriverName { get; set; }

        public string DriverSurnane { get; set; }

        public string Signature { get; set; }

        public string filePath { get; set; }

        [ForeignKey("AssDrivId")]
        public virtual DriverAssignment DriverAssignment { get; set; }
    }
}