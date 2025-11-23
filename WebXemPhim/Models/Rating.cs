using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        [Range(1, 5)]
        public int Value { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int? FilmId { get; set; }
        public virtual Film Film { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}