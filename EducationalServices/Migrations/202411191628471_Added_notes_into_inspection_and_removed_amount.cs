namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_notes_into_inspection_and_removed_amount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookInspections", "Notes", c => c.String());
            DropColumn("dbo.BookInspections", "Image");
            DropColumn("dbo.BookInspections", "Amount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookInspections", "Amount", c => c.Double(nullable: false));
            AddColumn("dbo.BookInspections", "Image", c => c.String());
            DropColumn("dbo.BookInspections", "Notes");
        }
    }
}
