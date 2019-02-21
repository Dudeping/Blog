namespace BlogPlus.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CHa : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Letters", "From_Id", "dbo.Users");
            DropIndex("dbo.Letters", new[] { "From_Id" });
            AlterColumn("dbo.Letters", "From_Id", c => c.Int());
            CreateIndex("dbo.Letters", "From_Id");
            AddForeignKey("dbo.Letters", "From_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Letters", "From_Id", "dbo.Users");
            DropIndex("dbo.Letters", new[] { "From_Id" });
            AlterColumn("dbo.Letters", "From_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Letters", "From_Id");
            AddForeignKey("dbo.Letters", "From_Id", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
