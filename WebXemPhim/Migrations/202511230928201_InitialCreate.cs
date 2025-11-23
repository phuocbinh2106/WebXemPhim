namespace WebXemPhim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        FilmId = c.Int(),
                        EpisodeId = c.Int(),
                        Content = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Episodes", t => t.EpisodeId)
                .ForeignKey("dbo.Films", t => t.FilmId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.FilmId)
                .Index(t => t.EpisodeId);
            
            CreateTable(
                "dbo.Episodes",
                c => new
                    {
                        EpisodeId = c.Int(nullable: false, identity: true),
                        EpisodeNumber = c.Int(nullable: false),
                        Title = c.String(),
                        VideoUrl = c.String(),
                        PublishedAt = c.DateTime(),
                        ViewCount = c.Int(nullable: false),
                        FilmId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EpisodeId)
                .ForeignKey("dbo.Films", t => t.FilmId, cascadeDelete: true)
                .Index(t => t.FilmId);
            
            CreateTable(
                "dbo.Films",
                c => new
                    {
                        FilmId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 255),
                        OriginalTitle = c.String(),
                        Description = c.String(),
                        ReleaseYear = c.Int(),
                        IsMovie = c.Boolean(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        EpsisodeCount = c.Int(),
                        ThumbnailUrl = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdateAt = c.DateTime(),
                        ViewCount = c.Int(nullable: false),
                        AverageRating = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.FilmId);
            
            CreateTable(
                "dbo.Favourites",
                c => new
                    {
                        FavouriteId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        FilmId = c.Int(nullable: false),
                        AddedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FavouriteId)
                .ForeignKey("dbo.Films", t => t.FilmId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.FilmId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 100),
                        Nickname = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        AboutMe = c.String(),
                        Role = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        Avatar = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.WatchHistories",
                c => new
                    {
                        WatchHistoryId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        EpisodeId = c.Int(nullable: false),
                        WatchedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WatchHistoryId)
                .ForeignKey("dbo.Episodes", t => t.EpisodeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.EpisodeId);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        GenreId = c.Int(nullable: false, identity: true),
                        GenreName = c.String(),
                    })
                .PrimaryKey(t => t.GenreId);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        RatingId = c.Int(nullable: false, identity: true),
                        Value = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        FilmId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RatingId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Films", t => t.FilmId, cascadeDelete: true)
                .Index(t => t.FilmId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Schedules",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false, identity: true),
                        DayOfWeek = c.Int(nullable: false),
                        Time = c.Time(precision: 7),
                        NextEpisodeDate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        FilmId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ScheduleId)
                .ForeignKey("dbo.Films", t => t.FilmId, cascadeDelete: true)
                .Index(t => t.FilmId);
            
            CreateTable(
                "dbo.VideoSources",
                c => new
                    {
                        VideoSourceId = c.Int(nullable: false, identity: true),
                        SeverName = c.String(),
                        Url = c.String(),
                        Quality = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        EpisodeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VideoSourceId)
                .ForeignKey("dbo.Episodes", t => t.EpisodeId, cascadeDelete: true)
                .Index(t => t.EpisodeId);
            
            CreateTable(
                "dbo.GenreFilms",
                c => new
                    {
                        Genre_GenreId = c.Int(nullable: false),
                        Film_FilmId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Genre_GenreId, t.Film_FilmId })
                .ForeignKey("dbo.Genres", t => t.Genre_GenreId, cascadeDelete: true)
                .ForeignKey("dbo.Films", t => t.Film_FilmId, cascadeDelete: true)
                .Index(t => t.Genre_GenreId)
                .Index(t => t.Film_FilmId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VideoSources", "EpisodeId", "dbo.Episodes");
            DropForeignKey("dbo.Schedules", "FilmId", "dbo.Films");
            DropForeignKey("dbo.Ratings", "FilmId", "dbo.Films");
            DropForeignKey("dbo.Ratings", "UserId", "dbo.Users");
            DropForeignKey("dbo.GenreFilms", "Film_FilmId", "dbo.Films");
            DropForeignKey("dbo.GenreFilms", "Genre_GenreId", "dbo.Genres");
            DropForeignKey("dbo.WatchHistories", "UserId", "dbo.Users");
            DropForeignKey("dbo.WatchHistories", "EpisodeId", "dbo.Episodes");
            DropForeignKey("dbo.Favourites", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Favourites", "FilmId", "dbo.Films");
            DropForeignKey("dbo.Episodes", "FilmId", "dbo.Films");
            DropForeignKey("dbo.Comments", "FilmId", "dbo.Films");
            DropForeignKey("dbo.Comments", "EpisodeId", "dbo.Episodes");
            DropIndex("dbo.GenreFilms", new[] { "Film_FilmId" });
            DropIndex("dbo.GenreFilms", new[] { "Genre_GenreId" });
            DropIndex("dbo.VideoSources", new[] { "EpisodeId" });
            DropIndex("dbo.Schedules", new[] { "FilmId" });
            DropIndex("dbo.Ratings", new[] { "UserId" });
            DropIndex("dbo.Ratings", new[] { "FilmId" });
            DropIndex("dbo.WatchHistories", new[] { "EpisodeId" });
            DropIndex("dbo.WatchHistories", new[] { "UserId" });
            DropIndex("dbo.Favourites", new[] { "FilmId" });
            DropIndex("dbo.Favourites", new[] { "UserId" });
            DropIndex("dbo.Episodes", new[] { "FilmId" });
            DropIndex("dbo.Comments", new[] { "EpisodeId" });
            DropIndex("dbo.Comments", new[] { "FilmId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropTable("dbo.GenreFilms");
            DropTable("dbo.VideoSources");
            DropTable("dbo.Schedules");
            DropTable("dbo.Ratings");
            DropTable("dbo.Genres");
            DropTable("dbo.WatchHistories");
            DropTable("dbo.Users");
            DropTable("dbo.Favourites");
            DropTable("dbo.Films");
            DropTable("dbo.Episodes");
            DropTable("dbo.Comments");
        }
    }
}
