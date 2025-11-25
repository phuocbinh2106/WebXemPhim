namespace WebXemPhim.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
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
