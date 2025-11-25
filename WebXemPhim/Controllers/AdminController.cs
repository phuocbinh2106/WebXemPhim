using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;
using WebXemPhim.ViewModel;

namespace WebXemPhim.Controllers
{
    public class AdminController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin
        // Hiển thị danh sách phim
        public ActionResult Index()
            {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
                var films = db.Films.ToList(); // Lấy danh sách phim
                var episodes = db.Episodes.ToList(); // Lấy danh sách tập phim
                var genres = db.Genres.ToList(); // Lấy danh sách thể loại
                var ViewModel = new AdminIndexViewModel
                {
                    Films = films,
                    Episodes = episodes,
                    Genres = genres
                };
                return View(ViewModel);
            }
    }
}