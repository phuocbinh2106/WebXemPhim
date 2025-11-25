using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;

namespace WebXemPhim.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();
        public ActionResult Index()
        {
            // Lấy 10 phim mới nhất theo ngày tạo (CreatedAt) hoặc theo ID nếu không có trường này
            var films = db.Films
                .OrderByDescending(f => f.CreatedAt) // Nếu không có CreatedAt => OrderByDescending(f => f.FilmId)
                .Take(10)
                .ToList();
            return View(films); // Sử dụng model là List<Film>
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}