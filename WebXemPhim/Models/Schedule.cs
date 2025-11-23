using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebXemPhim.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // Thứ mấy trong tuần
        public TimeSpan? Time { get; set; } //Giờ Chiếu
        public DateTime? NextEpisodeDate { get; set; } //Ngày Chiếu Tập Mới
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int FilmId { get; set; }
        public virtual Film Film { get; set; }
    }
}