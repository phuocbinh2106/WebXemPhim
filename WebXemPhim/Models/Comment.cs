using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int? FilmId { get; set; }
        public virtual Film Film { get; set; }
        public int? EpisodeId { get; set; }
        public virtual Episode Episode { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}