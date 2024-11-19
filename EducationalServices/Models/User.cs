﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EducationalServices.Models
{
    public enum RoleEnum
    {
        Driver,
        
    }
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public RoleEnum Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
       
    }

}