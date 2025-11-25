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
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var admin = new User
                {
                    Username = "admin",
                    Password = "123123",
                    Email = "email@domain.com",
                    Role = "Admin"
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
