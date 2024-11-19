// EducationalServices, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// EducationalServices.Models.AddPhoneNumberViewModel
using System.ComponentModel.DataAnnotations;

public class AddPhoneNumberViewModel
{
    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string Number { get; set; }
}
