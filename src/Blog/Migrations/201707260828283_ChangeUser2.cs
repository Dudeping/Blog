namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeUser2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "BlogLookNum", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "BlogLookNum");
        }
    }
}
