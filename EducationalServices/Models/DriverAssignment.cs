// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Models.DriverAssignment
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EducationalServices.Models;
using PayPal.Api;

public class DriverAssignment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AssDrivId { get; set; }

    [DisplayName("Order Number")]
    public int OrderId { get; set; }

    public int DrivId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public string Status { get; set; }

    [DataType(DataType.Date)]
    public string DeliveryDate { get; set; }

    [DataType(DataType.Time)]
    public string DeliveryTime { get; set; }

    public DateTime Created { get; set; }

    public bool IsActive { get; set; }

    [DisplayName("Due on")]
    public string GenDeliveryDate { get; set; }

    [DisplayName("Selected Time")]
    public string preffaredTime { get; set; }

    [DisplayName("Delivery Address")]
    public string DeliveryAddress { get; set; }

    [ForeignKey("OrderId")]
    public virtual Orders Orders { get; set; }

    [ForeignKey("DrivId")]
    public virtual Driver Driver { get; set; }
}
