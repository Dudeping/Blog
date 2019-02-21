namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Crea : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 50),
                        Content = c.String(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        EidtTime = c.DateTime(nullable: false),
                        CollectionTimes = c.Int(nullable: false),
                        LookNum = c.Int(nullable: false),
                        Fabulou = c.Int(nullable: false),
                        BlogType = c.String(nullable: false),
                        IsRelease = c.Boolean(nullable: false),
                        IsPulish = c.Boolean(nullable: false),
                        IsPush = c.Boolean(nullable: false),
                        Author_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Author_Id)
                .Index(t => t.Author_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 255),
                        NickName = c.String(nullable: false, maxLength: 255),
                        Belief = c.String(maxLength: 500),
                        JoinTime = c.DateTime(nullable: false),
                        LookNum = c.Int(nullable: false),
                        CreateNum = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        SchoolName = c.String(),
                        PicLink = c.String(),
                        FriendLink = c.String(),
                        IsPubulish = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Letters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 50),
                        Content = c.String(nullable: false, maxLength: 1000),
                        IsRead = c.Boolean(nullable: false),
                        To = c.String(nullable: false, maxLength: 255),
                        LetterType = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        From_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.From_Id, cascadeDelete: true)
                .Index(t => t.From_Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 255),
                        Password = c.String(nullable: false, maxLength: 40),
                        ErrorTimes = c.Int(nullable: false),
                        LockingTime = c.DateTime(nullable: false),
                        RCode = c.String(),
                        Invalid = c.DateTime(nullable: false),
                        FCode = c.String(),
                        Role_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.Role_Id)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "dbo.TagBlogs",
                c => new
                    {
                        Tag_Id = c.Int(nullable: false),
                        Blog_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Blog_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Blogs", t => t.Blog_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.Blog_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserLogins", "Role_Id", "dbo.Roles");
            DropForeignKey("dbo.Tags", "User_Id", "dbo.Users");
            DropForeignKey("dbo.TagBlogs", "Blog_Id", "dbo.Blogs");
            DropForeignKey("dbo.TagBlogs", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.Letters", "From_Id", "dbo.Users");
            DropForeignKey("dbo.Blogs", "Author_Id", "dbo.Users");
            DropIndex("dbo.TagBlogs", new[] { "Blog_Id" });
            DropIndex("dbo.TagBlogs", new[] { "Tag_Id" });
            DropIndex("dbo.UserLogins", new[] { "Role_Id" });
            DropIndex("dbo.Tags", new[] { "User_Id" });
            DropIndex("dbo.Letters", new[] { "From_Id" });
            DropIndex("dbo.Blogs", new[] { "Author_Id" });
            DropTable("dbo.TagBlogs");
            DropTable("dbo.UserLogins");
            DropTable("dbo.Roles");
            DropTable("dbo.Tags");
            DropTable("dbo.Letters");
            DropTable("dbo.Users");
            DropTable("dbo.Blogs");
        }
    }
}
