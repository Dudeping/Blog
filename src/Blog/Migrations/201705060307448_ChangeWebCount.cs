namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeWebCount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WebConfigs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LookNums = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WebConfigs");
        }
    }
}
