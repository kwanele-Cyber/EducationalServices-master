namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_BookStatusEnum : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Books", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Books", "Status", c => c.String(maxLength: 50));
        }
    }
}
