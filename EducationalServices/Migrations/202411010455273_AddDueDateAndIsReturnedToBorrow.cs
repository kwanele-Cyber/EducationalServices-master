namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDueDateAndIsReturnedToBorrow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrows", "IsReturned", c => c.Boolean(nullable: false));
            AddColumn("dbo.Borrows", "DueDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Borrows", "DueDate");
            DropColumn("dbo.Borrows", "IsReturned");
        }
    }
}
