using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDbContext")
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<Film> Films { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<WatchHistory> WatchHistories { get; set; }
        public DbSet<VideoSource> VideoSources { get; set; }
        public object FilmGenres { get; internal set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Film - Episode ( 1 - n )
            modelBuilder.Entity<Film>()
                .HasMany(f => f.Episodes)
                .WithRequired(e => e.Film)
                .HasForeignKey(e => e.FilmId)
                .WillCascadeOnDelete(true);

            //Epsiode - VideoSource ( 1 - n )
            modelBuilder.Entity<Episode>()
                .HasMany(e => e.VideoSources)
                .WithRequired(v => v.Episode)
                .HasForeignKey(v => v.EpisodeId)
                .WillCascadeOnDelete(true);

            //User - Comment ( 1 - n )
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithRequired(c => c.User)
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(true);

            //Film - Comment ( 1 - n )
            modelBuilder.Entity<Film>()
                .HasMany(f => f.Comments)
                .WithOptional(c => c.Film)
                .HasForeignKey(c => c.FilmId)
                .WillCascadeOnDelete(false);

            //Episode - Comment ( 1 - n )
            modelBuilder.Entity<Episode>()
                .HasMany(e => e.Comments)
                .WithOptional(c => c.Episode)
                .HasForeignKey(c => c.EpisodeId)
                .WillCascadeOnDelete(false);

            //User - Favourite ( 1 - n )
            modelBuilder.Entity<User>()
                .HasMany(u => u.Favourites)
                .WithRequired(f => f.User)
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(true);

            //Film - Favourite ( 1 - n )
            modelBuilder.Entity<Film>()
                .HasMany(f => f.Ratings)
                .WithRequired(r => r.Film)
                .HasForeignKey(r => r.FilmId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}