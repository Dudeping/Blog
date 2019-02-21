namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeCollection : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Collections",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId_Id = c.Int(nullable: false),
                        UserId_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blogs", t => t.BlogId_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId_Id, cascadeDelete: true)
                .Index(t => t.BlogId_Id)
                .Index(t => t.UserId_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Collections", "UserId_Id", "dbo.Users");
            DropForeignKey("dbo.Collections", "BlogId_Id", "dbo.Blogs");
            DropIndex("dbo.Collections", new[] { "UserId_Id" });
            DropIndex("dbo.Collections", new[] { "BlogId_Id" });
            DropTable("dbo.Collections");
        }
    }
}
