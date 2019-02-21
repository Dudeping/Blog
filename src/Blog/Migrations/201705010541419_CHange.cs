namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CHange : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        LogType = c.String(),
                        Content = c.String(),
                        IsRead = c.Boolean(nullable: false),
                        User = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        Ip = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SysNews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 20),
                        Content = c.String(nullable: false, maxLength: 1000),
                        CreateTime = c.DateTime(nullable: false),
                        UserLogin_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserLogins", t => t.UserLogin_Id)
                .Index(t => t.UserLogin_Id);
            
            AddColumn("dbo.Users", "SysNews_Id", c => c.Int());
            AddColumn("dbo.Letters", "Reply", c => c.String());
            AddColumn("dbo.Letters", "IsReadReply", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Users", "SysNews_Id");
            AddForeignKey("dbo.Users", "SysNews_Id", "dbo.SysNews", "Id");
            DropColumn("dbo.Letters", "LetterType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Letters", "LetterType", c => c.Int(nullable: false));
            DropForeignKey("dbo.SysNews", "UserLogin_Id", "dbo.UserLogins");
            DropForeignKey("dbo.Users", "SysNews_Id", "dbo.SysNews");
            DropIndex("dbo.SysNews", new[] { "UserLogin_Id" });
            DropIndex("dbo.Users", new[] { "SysNews_Id" });
            DropColumn("dbo.Letters", "IsReadReply");
            DropColumn("dbo.Letters", "Reply");
            DropColumn("dbo.Users", "SysNews_Id");
            DropTable("dbo.SysNews");
            DropTable("dbo.Logs");
        }
    }
}
