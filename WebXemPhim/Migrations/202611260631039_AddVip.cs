namespace WebXemPhim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVip : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VipTransactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.String(nullable: false, maxLength: 20),
                        Description = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Users", "Balance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Users", "VipExpiresAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VipTransactions", "UserId", "dbo.Users");
            DropIndex("dbo.VipTransactions", new[] { "UserId" });
            DropColumn("dbo.Users", "VipExpiresAt");
            DropColumn("dbo.Users", "Balance");
            DropTable("dbo.VipTransactions");
        }
    }
}
