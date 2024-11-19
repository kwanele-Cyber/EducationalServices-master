using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalServices.Models
{
    public partial class BookInspection
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }

        public string UserId { get; set; }
        public int BorrowId { get; set; }

        public BookInspectionStatus Status { get; set; }

        public string Notes { get; set; }



        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; }
        [ForeignKey(nameof(BorrowId))]
        public virtual Borrow Borrow { get; set; }
    }
}