namespace EducationalServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookOrders",
                c => new
                    {
                        BookOrderId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Author = c.String(nullable: false, maxLength: 100),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderDate = c.DateTime(nullable: false),
                        IsFulfilled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BookOrderId);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        BookId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Author = c.String(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                        ImagePath = c.String(),
                    })
                .PrimaryKey(t => t.BookId);
            
            CreateTable(
                "dbo.Borrows",
                c => new
                    {
                        BorrowId = c.Int(nullable: false, identity: true),
                        BookId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        BorrowDate = c.DateTime(nullable: false),
                        ReturnDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BorrowId)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.BookId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.QuizRatings",
                c => new
                    {
                        RatingId = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        StudentId = c.String(nullable: false, maxLength: 128),
                        QuizAttemptId = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(maxLength: 500),
                        DateRated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RatingId)
                .ForeignKey("dbo.QuizAttempts", t => t.QuizAttemptId)
                .ForeignKey("dbo.Quizs", t => t.QuizId)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.StudentId)
                .Index(t => t.QuizAttemptId);
            
            CreateTable(
                "dbo.Quizs",
                c => new
                    {
                        QuizId = c.Int(nullable: false, identity: true),
                        ModuleId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 1000),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        MaxAttempts = c.Int(nullable: false),
                        TimeLimit = c.Int(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuizId)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .Index(t => t.ModuleId);
            
            CreateTable(
                "dbo.QuizAttempts",
                c => new
                    {
                        AttemptId = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        StudentId = c.String(nullable: false, maxLength: 128),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(),
                        Score = c.Int(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        QuizRating_RatingId = c.Int(),
                    })
                .PrimaryKey(t => t.AttemptId)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .ForeignKey("dbo.QuizRatings", t => t.QuizRating_RatingId)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.StudentId)
                .Index(t => t.QuizRating_RatingId);
            
            CreateTable(
                "dbo.StudentAnswers",
                c => new
                    {
                        AnswerId = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        Answer = c.String(nullable: false, maxLength: 2000),
                        MarksObtained = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.QuizAttempts", t => t.QuizAttemptId)
                .Index(t => t.QuizAttemptId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false, maxLength: 1000),
                        Options = c.String(maxLength: 2000),
                        CorrectAnswer = c.String(nullable: false, maxLength: 1000),
                        Marks = c.Int(nullable: false),
                        QuestionType = c.Int(nullable: false),
                        QuizId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.Modules",
                c => new
                    {
                        ModuleId = c.Int(nullable: false, identity: true),
                        Subject = c.String(nullable: false, maxLength: 100),
                        SubjectCode = c.String(nullable: false, maxLength: 20),
                        Description = c.String(maxLength: 1000),
                        DurationInHours = c.Int(nullable: false),
                        Difficulty = c.String(maxLength: 20),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ModuleId);
            
            CreateTable(
                "dbo.TutorModules",
                c => new
                    {
                        TutorModuleId = c.Int(nullable: false, identity: true),
                        ModuleId = c.Int(nullable: false),
                        TutorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.TutorModuleId)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TutorId, cascadeDelete: true)
                .Index(t => t.ModuleId)
                .Index(t => t.TutorId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        ReservationId = c.Int(nullable: false, identity: true),
                        BookId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        ReservationDate = c.DateTime(nullable: false),
                        ReservationOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReservationId)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.BookId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        CardId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Number = c.String(nullable: false),
                        ExpirationDate = c.String(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.CardId);
            
            CreateTable(
                "dbo.CourseRecommendations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.String(maxLength: 128),
                        ModuleId = c.Int(nullable: false),
                        Reason = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId)
                .Index(t => t.StudentId)
                .Index(t => t.ModuleId);
            
            CreateTable(
                "dbo.CustInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Surname = c.String(),
                        Address = c.String(),
                        Email = c.String(),
                        RecipientName = c.String(),
                        RecipientNumber = c.String(),
                        deliveryDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        preffaredTime = c.String(),
                        ShippingMethod = c.String(),
                        deliveryFee = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DigitalResources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Author = c.String(),
                        Type = c.String(),
                        FileUrl = c.String(),
                        Description = c.String(),
                        DateAdded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DispatchRecords",
                c => new
                    {
                        DisID = c.Int(nullable: false, identity: true),
                        AssDrivId = c.Int(nullable: false),
                        OrderNumber = c.Int(nullable: false),
                        DriverName = c.String(),
                        DriverSurnane = c.String(),
                        Signature = c.String(),
                        filePath = c.String(),
                    })
                .PrimaryKey(t => t.DisID)
                .ForeignKey("dbo.DriverAssignments", t => t.AssDrivId, cascadeDelete: true)
                .Index(t => t.AssDrivId);
            
            CreateTable(
                "dbo.DriverAssignments",
                c => new
                    {
                        AssDrivId = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        DrivId = c.Int(nullable: false),
                        Name = c.String(),
                        Surname = c.String(),
                        Email = c.String(),
                        Status = c.String(),
                        DeliveryDate = c.String(),
                        DeliveryTime = c.String(),
                        Created = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        GenDeliveryDate = c.String(),
                        preffaredTime = c.String(),
                        DeliveryAddress = c.String(),
                    })
                .PrimaryKey(t => t.AssDrivId)
                .ForeignKey("dbo.Drivers", t => t.DrivId, cascadeDelete: true)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.DrivId);
            
            CreateTable(
                "dbo.Drivers",
                c => new
                    {
                        DrivId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Surname = c.String(),
                        Email = c.String(),
                        Picture = c.String(),
                        IsAvailable = c.Boolean(nullable: false),
                        CarName = c.String(),
                        CarModel = c.String(),
                        CarReg = c.String(),
                        CarType = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        Capacity = c.String(),
                    })
                .PrimaryKey(t => t.DrivId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        OrderDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        deliveryDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DeliveredOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        PickupDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.String(),
                        Code = c.Int(nullable: false),
                        DeliveryFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProductCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Address = c.String(),
                        Email = c.String(),
                        DriverEmail = c.String(),
                        DeliveredBy = c.Int(nullable: false),
                        IsDeliveryRescheduled = c.Boolean(nullable: false),
                        PaymentID = c.String(),
                    })
                .PrimaryKey(t => t.OrderId);
            
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderDetailsId = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StockQuantity = c.Int(nullable: false),
                        Weight = c.String(),
                        Picture = c.String(),
                    })
                .PrimaryKey(t => t.OrderDetailsId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StockQuantity = c.Int(nullable: false),
                        Weight = c.String(nullable: false),
                        Picture = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.RoomBookings",
                c => new
                    {
                        RoomBookingId = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RoomBookingId)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.RoomId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        RoomName = c.String(),
                        Capacity = c.Int(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RoomId);
            
            CreateTable(
                "dbo.StudentModules",
                c => new
                    {
                        StudentModuleId = c.Int(nullable: false, identity: true),
                        ModuleId = c.Int(nullable: false),
                        StudentId = c.String(nullable: false, maxLength: 128),
                        Status = c.String(maxLength: 50),
                        TotalPaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.StudentModuleId)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.ModuleId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.StudyRoomBookings",
                c => new
                    {
                        StudyRoomBookingId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        StartTime = c.DateTime(nullable: false),
                        DurationInHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StudyRoomBookingId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.SupportRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.String(maxLength: 128),
                        Issue = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        Status = c.String(),
                        Resolution = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId)
                .Index(t => t.StudentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SupportRequests", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StudyRoomBookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StudentModules", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StudentModules", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.RoomBookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RoomBookings", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.OrderDetails", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.DispatchRecords", "AssDrivId", "dbo.DriverAssignments");
            DropForeignKey("dbo.DriverAssignments", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.DriverAssignments", "DrivId", "dbo.Drivers");
            DropForeignKey("dbo.CourseRecommendations", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CourseRecommendations", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.Reservations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reservations", "BookId", "dbo.Books");
            DropForeignKey("dbo.Borrows", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.QuizRatings", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.QuizRatings", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.TutorModules", "TutorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TutorModules", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.Quizs", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.QuizAttempts", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.QuizRatings", "QuizAttemptId", "dbo.QuizAttempts");
            DropForeignKey("dbo.QuizAttempts", "QuizRating_RatingId", "dbo.QuizRatings");
            DropForeignKey("dbo.QuizAttempts", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.StudentAnswers", "QuizAttemptId", "dbo.QuizAttempts");
            DropForeignKey("dbo.StudentAnswers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Questions", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Borrows", "BookId", "dbo.Books");
            DropIndex("dbo.SupportRequests", new[] { "StudentId" });
            DropIndex("dbo.StudyRoomBookings", new[] { "UserId" });
            DropIndex("dbo.StudentModules", new[] { "StudentId" });
            DropIndex("dbo.StudentModules", new[] { "ModuleId" });
            DropIndex("dbo.RoomBookings", new[] { "UserId" });
            DropIndex("dbo.RoomBookings", new[] { "RoomId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.OrderDetails", new[] { "OrderId" });
            DropIndex("dbo.DriverAssignments", new[] { "DrivId" });
            DropIndex("dbo.DriverAssignments", new[] { "OrderId" });
            DropIndex("dbo.DispatchRecords", new[] { "AssDrivId" });
            DropIndex("dbo.CourseRecommendations", new[] { "ModuleId" });
            DropIndex("dbo.CourseRecommendations", new[] { "StudentId" });
            DropIndex("dbo.Reservations", new[] { "UserId" });
            DropIndex("dbo.Reservations", new[] { "BookId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.TutorModules", new[] { "TutorId" });
            DropIndex("dbo.TutorModules", new[] { "ModuleId" });
            DropIndex("dbo.Questions", new[] { "QuizId" });
            DropIndex("dbo.StudentAnswers", new[] { "QuestionId" });
            DropIndex("dbo.StudentAnswers", new[] { "QuizAttemptId" });
            DropIndex("dbo.QuizAttempts", new[] { "QuizRating_RatingId" });
            DropIndex("dbo.QuizAttempts", new[] { "StudentId" });
            DropIndex("dbo.QuizAttempts", new[] { "QuizId" });
            DropIndex("dbo.Quizs", new[] { "ModuleId" });
            DropIndex("dbo.QuizRatings", new[] { "QuizAttemptId" });
            DropIndex("dbo.QuizRatings", new[] { "StudentId" });
            DropIndex("dbo.QuizRatings", new[] { "QuizId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Borrows", new[] { "UserId" });
            DropIndex("dbo.Borrows", new[] { "BookId" });
            DropTable("dbo.SupportRequests");
            DropTable("dbo.StudyRoomBookings");
            DropTable("dbo.StudentModules");
            DropTable("dbo.Rooms");
            DropTable("dbo.RoomBookings");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Products");
            DropTable("dbo.OrderDetails");
            DropTable("dbo.Orders");
            DropTable("dbo.Drivers");
            DropTable("dbo.DriverAssignments");
            DropTable("dbo.DispatchRecords");
            DropTable("dbo.DigitalResources");
            DropTable("dbo.CustInfoes");
            DropTable("dbo.CourseRecommendations");
            DropTable("dbo.Cards");
            DropTable("dbo.Reservations");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.TutorModules");
            DropTable("dbo.Modules");
            DropTable("dbo.Questions");
            DropTable("dbo.StudentAnswers");
            DropTable("dbo.QuizAttempts");
            DropTable("dbo.Quizs");
            DropTable("dbo.QuizRatings");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Borrows");
            DropTable("dbo.Books");
            DropTable("dbo.BookOrders");
        }
    }
}
