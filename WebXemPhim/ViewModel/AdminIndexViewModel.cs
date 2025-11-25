using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebXemPhim.Models;

namespace WebXemPhim.ViewModel
{
    public class AdminIndexViewModel
    {
        public List<Film> Films { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<Genre> Genres { get; set; }
    }
}