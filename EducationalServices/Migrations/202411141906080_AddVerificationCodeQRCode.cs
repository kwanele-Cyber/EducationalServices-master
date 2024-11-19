namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVerificationCodeQRCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VerificationCodes", "Base64Img", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VerificationCodes", "Base64Img");
        }
    }
}
