using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationalServices.Models
{
    public class VerificationCode
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string UserId { get; set; }
        public string Base64Img { get; set; }
        public VerificationCodeStatus Status { get; set; }

        public VerificationCode()
        {
            Id = Guid.NewGuid();
            Code = new Random().Next(10000, 99999).ToString();
            Status = VerificationCodeStatus.NOT_VERIFIED;
        }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
    }

    public enum VerificationCodeStatus{
        NOT_VERIFIED,
        VERIFIED,
    }
}
