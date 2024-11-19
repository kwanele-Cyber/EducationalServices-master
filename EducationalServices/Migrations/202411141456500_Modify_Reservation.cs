namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Modify_Reservation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "ReservationEndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reservations", "ReservationEndDate");
        }
    }
}
