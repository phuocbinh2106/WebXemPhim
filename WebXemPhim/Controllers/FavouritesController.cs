using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;
using System.Data.Entity;


namespace WebXemPhim.Controllers
{
    public class FavouritesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Favourites/Index
        public ActionResult Index()
        {
            object sessionValue = Session["UserId"];
            if (sessionValue == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(sessionValue);

            // Sử dụng Include với lambda cho đúng chuẩn và tránh proxy lỗi
            var list = db.Favourites
                .Include(f => f.Film)
                .Where(f => f.UserId == userId)
                .ToList();

            return View(list);
        }


        // POST: Favourites/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int filmId)
        {
            var role = Session["Role"]?.ToString().ToLower();
            if (role != "admin" && role != "user")
                return RedirectToAction("Index", "Home");

            object sessionValue = Session["UserId"];
            if (sessionValue == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(sessionValue);
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var exists = db.Favourites.Any(f => f.UserId == userId && f.FilmId == filmId);
            if (!exists)
            {
                db.Favourites.Add(new Favourite
                {
                    UserId = userId,
                    FilmId = filmId
                });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
