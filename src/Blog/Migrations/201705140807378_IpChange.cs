namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IpChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "IpLocation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Logs", "IpLocation");
        }
    }
}
