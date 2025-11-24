namespace WebXemPhim.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using WebXemPhim.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebXemPhim.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebXemPhim.Models.AppDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            // Tạo role "Admin" nếu chưa tồn tại
            var adminUser = new User
            {
                Username = "admin",
                Password = "123123",
                Email = "admin@webxemphim.com",
                Role = "Admin",
                CreatedAt = DateTime.Now
            };

            // Kiểm tra trước khi tạo tránh trùng
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

        }
    }
}
