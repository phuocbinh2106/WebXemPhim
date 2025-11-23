using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required, StringLength(100)]
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string AboutMe { get; set; }
        public string Role { get; set; } = "User"; // User or Admin
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Avatar { get; set; }
        //Navigation
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<WatchHistory> WatchHistories { get; set; }
    }
}