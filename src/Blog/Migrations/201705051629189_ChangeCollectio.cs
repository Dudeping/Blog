namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeCollectio : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Collections", "BlogId_Id", "dbo.Blogs");
            DropIndex("dbo.Collections", new[] { "BlogId_Id" });
            AlterColumn("dbo.Collections", "BlogId_Id", c => c.Int());
            CreateIndex("dbo.Collections", "BlogId_Id");
            AddForeignKey("dbo.Collections", "BlogId_Id", "dbo.Blogs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Collections", "BlogId_Id", "dbo.Blogs");
            DropIndex("dbo.Collections", new[] { "BlogId_Id" });
            AlterColumn("dbo.Collections", "BlogId_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Collections", "BlogId_Id");
            AddForeignKey("dbo.Collections", "BlogId_Id", "dbo.Blogs", "Id", cascadeDelete: true);
        }
    }
}
