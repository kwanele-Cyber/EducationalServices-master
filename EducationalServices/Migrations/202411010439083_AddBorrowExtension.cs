namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBorrowExtension : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BorrowExtensions",
                c => new
                    {
                        BorrowExtensionId = c.Int(nullable: false, identity: true),
                        BorrowId = c.Int(nullable: false),
                        ExtensionDate = c.DateTime(nullable: false),
                        DaysExtended = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.BorrowExtensionId)
                .ForeignKey("dbo.Borrows", t => t.BorrowId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.BorrowId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BorrowExtensions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BorrowExtensions", "BorrowId", "dbo.Borrows");
            DropIndex("dbo.BorrowExtensions", new[] { "UserId" });
            DropIndex("dbo.BorrowExtensions", new[] { "BorrowId" });
            DropTable("dbo.BorrowExtensions");
        }
    }
}
