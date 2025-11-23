using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Film
    {
        public int FilmId { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }
        public string OriginalTitle { get; set; } // Tên gốc
        public string Description { get; set; }

        public int? ReleaseYear { get; set; }
        public bool IsMovie { get; set; } = false; // true: Phim le, false: Phim bo
        public bool IsCompleted { get; set; } = false; // Hoàn thành hay đang chiếu

        public int? EpsisodeCount { get; set; } // Số tập (nếu là phim bộ)
        public string ThumbnailUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdateAt { get; set; }

        //Navigation
        public virtual ICollection<Genre> Genres { get; set; }
        public virtual ICollection<Episode> Episodes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }

        public int ViewCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
    }
}