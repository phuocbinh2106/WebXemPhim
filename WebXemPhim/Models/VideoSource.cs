using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class VideoSource
    {
        public int VideoSourceId { get; set; }
        public string SeverName { get; set; }
        public string Url { get; set; }
        public int Quality { get; set; } // 360, 480, 720, 1080
        public bool IsActive { get; set; } = true;

        // Foreign Key
        public int EpisodeId { get; set; }
        public virtual Episode Episode { get; set; }
    }
}