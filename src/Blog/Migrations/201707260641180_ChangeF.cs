namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeF : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blogs", "Brief", c => c.String());
            AddColumn("dbo.Users", "BlogCount", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "JottingNum", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "ArticleNum", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ArticleNum");
            DropColumn("dbo.Users", "JottingNum");
            DropColumn("dbo.Users", "BlogCount");
            DropColumn("dbo.Blogs", "Brief");
        }
    }
}
