using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan? Time { get; set; }
        public DateTime? NextEpisodeDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int FilmId { get; set; }
        public virtual Film Film { get; set; }
    }
}