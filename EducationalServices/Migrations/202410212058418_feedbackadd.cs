namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedbackadd : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        UserName = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Message = c.String(nullable: false, maxLength: 500),
                        DateSubmitted = c.DateTime(nullable: false),
                        IsResolved = c.Boolean(nullable: false),
                        AdminResponse = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Feedbacks");
        }
    }
}
