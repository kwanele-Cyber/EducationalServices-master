namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVerificationCodeQRCode1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.VerificationCodes", "UserId", "dbo.Users");
            DropIndex("dbo.VerificationCodes", new[] { "UserId" });
            AlterColumn("dbo.VerificationCodes", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.VerificationCodes", "UserId");
            AddForeignKey("dbo.VerificationCodes", "UserId", "dbo.AspNetUsers", "Id");
            DropTable("dbo.Users");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Role = c.Int(nullable: false),
                        Email = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.VerificationCodes", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.VerificationCodes", new[] { "UserId" });
            AlterColumn("dbo.VerificationCodes", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.VerificationCodes", "UserId");
            AddForeignKey("dbo.VerificationCodes", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
