using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
   public class CourseRecommendation
{
	public int Id { get; set; }

	public string StudentId { get; set; }

	public virtual ApplicationUser Student { get; set; }

	public int ModuleId { get; set; }

	public virtual Module Module { get; set; }

	public string Reason { get; set; }

	public DateTime CreatedAt { get; set; }
}

}