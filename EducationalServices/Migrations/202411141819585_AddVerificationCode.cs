namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVerificationCode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VerificationCodes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        UserId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
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
            
            AddColumn("dbo.Reservations", "VerificationCodeId", c => c.Guid());
            CreateIndex("dbo.Reservations", "VerificationCodeId");
            AddForeignKey("dbo.Reservations", "VerificationCodeId", "dbo.VerificationCodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reservations", "VerificationCodeId", "dbo.VerificationCodes");
            DropForeignKey("dbo.VerificationCodes", "UserId", "dbo.Users");
            DropIndex("dbo.VerificationCodes", new[] { "UserId" });
            DropIndex("dbo.Reservations", new[] { "VerificationCodeId" });
            DropColumn("dbo.Reservations", "VerificationCodeId");
            DropTable("dbo.Users");
            DropTable("dbo.VerificationCodes");
        }
    }
}
