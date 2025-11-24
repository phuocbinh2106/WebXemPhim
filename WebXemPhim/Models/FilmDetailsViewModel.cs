using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class FilmDetailsViewModel
    {
        public Film Film { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Rating> Ratings { get; set; }
    }
}