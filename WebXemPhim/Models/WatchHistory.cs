using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class WatchHistory
    {
        public int WatchHistoryId { get; set; }
        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int EpisodeId { get; set; }
        public virtual Episode Episode { get; set; }
        public DateTime WatchedAt { get; set; } = DateTime.Now;
    }
}