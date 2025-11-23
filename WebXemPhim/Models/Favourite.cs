using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Favourite
    {
        public int FavouriteId { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int FilmId { get; set; }
        public virtual Film Film { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}