namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBookInspection : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookInspections",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        BorrowId = c.Int(nullable: false),
                        Image = c.String(),
                        Status = c.Int(nullable: false),
                        Amount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Borrows", t => t.BorrowId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.BookId)
                .Index(t => t.UserId)
                .Index(t => t.BorrowId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookInspections", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BookInspections", "BorrowId", "dbo.Borrows");
            DropForeignKey("dbo.BookInspections", "BookId", "dbo.Books");
            DropIndex("dbo.BookInspections", new[] { "BorrowId" });
            DropIndex("dbo.BookInspections", new[] { "UserId" });
            DropIndex("dbo.BookInspections", new[] { "BookId" });
            DropTable("dbo.BookInspections");
        }
    }
}
