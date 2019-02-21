namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategory : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TagBlogs", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.TagBlogs", "Blog_Id", "dbo.Blogs");
            DropForeignKey("dbo.Tags", "User_Id", "dbo.Users");
            DropIndex("dbo.Tags", new[] { "User_Id" });
            DropIndex("dbo.TagBlogs", new[] { "Tag_Id" });
            DropIndex("dbo.TagBlogs", new[] { "Blog_Id" });
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        User = c.String(),
                        BlogCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CategoryBlogs",
                c => new
                    {
                        Category_Id = c.Int(nullable: false),
                        Blog_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Category_Id, t.Blog_Id })
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .ForeignKey("dbo.Blogs", t => t.Blog_Id, cascadeDelete: true)
                .Index(t => t.Category_Id)
                .Index(t => t.Blog_Id);
            
            AddColumn("dbo.Blogs", "Tags", c => c.String());
            DropTable("dbo.Tags");
            DropTable("dbo.TagBlogs");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TagBlogs",
                c => new
                    {
                        Tag_Id = c.Int(nullable: false),
                        Blog_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Blog_Id });
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.CategoryBlogs", "Blog_Id", "dbo.Blogs");
            DropForeignKey("dbo.CategoryBlogs", "Category_Id", "dbo.Categories");
            DropIndex("dbo.CategoryBlogs", new[] { "Blog_Id" });
            DropIndex("dbo.CategoryBlogs", new[] { "Category_Id" });
            DropColumn("dbo.Blogs", "Tags");
            DropTable("dbo.CategoryBlogs");
            DropTable("dbo.Categories");
            CreateIndex("dbo.TagBlogs", "Blog_Id");
            CreateIndex("dbo.TagBlogs", "Tag_Id");
            CreateIndex("dbo.Tags", "User_Id");
            AddForeignKey("dbo.Tags", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.TagBlogs", "Blog_Id", "dbo.Blogs", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TagBlogs", "Tag_Id", "dbo.Tags", "Id", cascadeDelete: true);
        }
    }
}
