using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Episode
    {
        public int EpisodeId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string VideoUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; } = 0;

        // Foreign Key
        public int FilmId { get; set; }
        public virtual Film Film { get; set; }

        //Nagivation
        public virtual ICollection<VideoSource> VideoSources { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}