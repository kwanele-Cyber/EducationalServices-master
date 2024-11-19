using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PayPal.Api;


namespace EducationalServices.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ICollection<QuizRating> QuizRatings { get; set; }
        public virtual ICollection<BookIssueReport> IssueReports { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            return await manager.CreateIdentityAsync(this, "ApplicationCookie");
        }


    }


  



  
        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public virtual DbSet<StudentModule> StudentModules { get; set; }
            public virtual DbSet<TutorModule> TutorModules { get; set; }
            public virtual DbSet<Module> Modules { get; set; }
            public virtual DbSet<OrderDetails> OrderDetails { get; set; }
            public virtual DbSet<Orders> Orders { get; set; }
            public virtual DbSet<Card> Cards { get; set; }
            public virtual DbSet<Product> Products { get; set; }
            public virtual DbSet<CustInfo> CustInfos { get; set; }
            public virtual DbSet<Driver> Drivers { get; set; }
            public virtual DbSet<DriverAssignment> DriverAssignments { get; set; }
            public virtual DbSet<DispatchRecord> DispatchRecords { get; set; }
            public DbSet<Quiz> Quizzes { get; set; }
            public DbSet<QuizAttempt> QuizAttempts { get; set; }
            public DbSet<SupportRequest> SupportRequests { get; set; }
            public DbSet<Question> Questions { get; set; }
            public DbSet<StudentAnswer> StudentAnswers { get; set; }
            public DbSet<QuizRating> QuizRatings { get; set; }
            public DbSet<CourseRecommendation> CourseRecommendations { get; set; }

            // Library entities
            public DbSet<Book> Books { get; set; }
            public DbSet<Reservation> Reservations { get; set; }
            public DbSet<Borrow> Borrows { get; set; }
            public DbSet<DigitalResource> DigitalResources { get; set; }
            public DbSet<StudyRoomBooking> StudyRoomBookings { get; set; }
            public DbSet<BookOrder> BookOrders { get; set; }
            public DbSet<Feedback> Feedbacks { get; set; }



        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomBooking> RoomBookings { get; set; }

        public DbSet<BookIssueReport> BookIssueReports { get; set; }



        public DbSet<BorrowExtension> BorrowExtensions { get; set; }

        public DbSet<VerificationCode> verificationCodes { get; set; }
        public DbSet<BookInspection> BookInspections { get; set; }


        public ApplicationDbContext()
                : base("DefaultConnection", throwIfV1Schema: false)
            {
                
            }

            public static ApplicationDbContext Create()
            {
                return new ApplicationDbContext();
            }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Disable cascading delete between StudentAnswer and QuizAttempt to avoid multiple cascade paths
            // Existing configurations
            modelBuilder.Entity<StudentAnswer>()
               .HasRequired(sa => sa.QuizAttempt)
               .WithMany(qa => qa.Answers)
               .HasForeignKey(sa => sa.QuizAttemptId)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuizAttempt>()
                .HasMany(a => a.QuizRatings)
                .WithRequired(r => r.QuizAttempt)
                .HasForeignKey(r => r.QuizAttemptId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.QuizRatings)
                .WithRequired(r => r.Quiz)
                .HasForeignKey(r => r.QuizId)
                .WillCascadeOnDelete(false);

            // Disable ALL cascade deletes for BookIssueReport related entities
            modelBuilder.Entity<BookIssueReport>()
                .HasRequired(r => r.Book)
                .WithMany(b => b.IssueReports)
                .HasForeignKey(r => r.BookId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BookIssueReport>()
                .HasRequired(r => r.Borrow)
                .WithMany(b => b.IssueReports)
                .HasForeignKey(r => r.BorrowId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BookIssueReport>()
                .HasRequired(r => r.User)
                .WithMany(u => u.IssueReports)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            // Disable cascade deletes for Borrow relationships
            modelBuilder.Entity<Borrow>()
                .HasRequired(b => b.Book)
                .WithMany(b => b.Borrows)
                .HasForeignKey(b => b.BookId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Borrow>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            // Configure decimal precision
            modelBuilder.Entity<BookIssueReport>()
                .Property(r => r.AssessedFee)
                .HasPrecision(10, 2);
        }


    }
}
