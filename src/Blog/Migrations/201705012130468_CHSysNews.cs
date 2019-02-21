namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CHSysNews : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "SysNews_Id", "dbo.SysNews");
            DropIndex("dbo.Users", new[] { "SysNews_Id" });
            CreateTable(
                "dbo.SysNewsUsers",
                c => new
                    {
                        SysNews_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SysNews_Id, t.User_Id })
                .ForeignKey("dbo.SysNews", t => t.SysNews_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.SysNews_Id)
                .Index(t => t.User_Id);
            
            DropColumn("dbo.Users", "SysNews_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "SysNews_Id", c => c.Int());
            DropForeignKey("dbo.SysNewsUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.SysNewsUsers", "SysNews_Id", "dbo.SysNews");
            DropIndex("dbo.SysNewsUsers", new[] { "User_Id" });
            DropIndex("dbo.SysNewsUsers", new[] { "SysNews_Id" });
            DropTable("dbo.SysNewsUsers");
            CreateIndex("dbo.Users", "SysNews_Id");
            AddForeignKey("dbo.Users", "SysNews_Id", "dbo.SysNews", "Id");
        }
    }
}
