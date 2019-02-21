namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeUser1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Letters", "IsReadReply");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Letters", "IsReadReply", c => c.Boolean(nullable: false));
        }
    }
}
