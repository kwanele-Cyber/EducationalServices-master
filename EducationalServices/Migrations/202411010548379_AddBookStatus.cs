namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddBookStatus : DbMigration
    {
        public override void Up()
        {
            // First drop any existing foreign key constraints if they exist
            DropForeignKey("dbo.BookIssueReports", "BorrowId", "dbo.Borrows");
            DropForeignKey("dbo.BookIssueReports", "BookId", "dbo.Books");
            DropForeignKey("dbo.BookIssueReports", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Borrows", "BookId", "dbo.Books");
            DropForeignKey("dbo.Borrows", "UserId", "dbo.AspNetUsers");

            // Create table if it doesn't exist
            CreateTable(
                "dbo.BookIssueReports",
                c => new
                {
                    ReportId = c.Int(nullable: false, identity: true),
                    BorrowId = c.Int(nullable: false),
                    BookId = c.Int(nullable: false),
                    UserId = c.String(nullable: false, maxLength: 128),
                    IssueType = c.String(nullable: false, maxLength: 50),
                    Description = c.String(nullable: false, maxLength: 1000),
                    ReportDate = c.DateTime(nullable: false),
                    Status = c.String(nullable: false, maxLength: 50),
                    AssessedFee = c.Decimal(precision: 10, scale: 2),
                    AdminComments = c.String(maxLength: 1000),
                    ResolutionDate = c.DateTime(),
                })
                .PrimaryKey(t => t.ReportId);

            // Add foreign keys with NO ACTION on delete
            AddForeignKey("dbo.BookIssueReports", "BorrowId", "dbo.Borrows", "BorrowId");
            AddForeignKey("dbo.BookIssueReports", "BookId", "dbo.Books", "BookId");
            AddForeignKey("dbo.BookIssueReports", "UserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Borrows", "BookId", "dbo.Books", "BookId");
            AddForeignKey("dbo.Borrows", "UserId", "dbo.AspNetUsers", "Id");

            // Add indexes
            CreateIndex("dbo.BookIssueReports", "BorrowId");
            CreateIndex("dbo.BookIssueReports", "BookId");
            CreateIndex("dbo.BookIssueReports", "UserId");

            // Add Status column to Books if it doesn't exist
            AddColumn("dbo.Books", "Status", c => c.String(maxLength: 50));
            Sql("UPDATE dbo.Books SET Status = 'Available' WHERE Status IS NULL");
        }

        public override void Down()
        {
            DropForeignKey("dbo.BookIssueReports", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BookIssueReports", "BorrowId", "dbo.Borrows");
            DropForeignKey("dbo.BookIssueReports", "BookId", "dbo.Books");
            DropIndex("dbo.BookIssueReports", new[] { "UserId" });
            DropIndex("dbo.BookIssueReports", new[] { "BorrowId" });
            DropIndex("dbo.BookIssueReports", new[] { "BookId" });
            DropTable("dbo.BookIssueReports");
            DropColumn("dbo.Books", "Status");
        }
    }
}
