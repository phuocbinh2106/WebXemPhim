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
                .OrderByDescending(f => f.AddedAt)
                .ToList();

            ViewBag.Message = TempData["FavouriteMessage"];
            return View(list);
        }


        private int CurrentUserId()
        {
            int userId;
            var value = Session["UserId"];
            return value != null && int.TryParse(value.ToString(), out userId) ? userId : 0;
        }

        // POST: Favourites/Add
        // SCRUM-33: khách chưa đăng nhập được chuyển tới trang Đăng nhập (trước đây về trang chủ).
        // SCRUM-30: filmId không tồn tại -> 404, không tạo bản ghi.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int? filmId)
        {
            int userId = CurrentUserId();
            if (userId == 0)
            {
                var back = filmId.HasValue ? Url.Action("Details", "Films", new { id = filmId }) : null;
                return RedirectToAction("Login", "Account", new { returnUrl = back });
            }

            if (filmId == null || !db.Films.Any(f => f.FilmId == filmId))
                return HttpNotFound();

            var exists = db.Favourites.Any(f => f.UserId == userId && f.FilmId == filmId);
            if (!exists)
            {
                db.Favourites.Add(new Favourite
                {
                    UserId = userId,
                    FilmId = filmId.Value,
                    AddedAt = DateTime.Now
                });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: Favourites/Remove
        // SCRUM-32: bỏ phim khỏi danh sách Yêu thích
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int? filmId, string returnUrl)
        {
            int userId = CurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            if (filmId.HasValue)
            {
                var items = db.Favourites.Where(f => f.UserId == userId && f.FilmId == filmId.Value).ToList();
                if (items.Count > 0)
                {
                    db.Favourites.RemoveRange(items);
                    db.SaveChanges();
                    TempData["FavouriteMessage"] = "Đã bỏ phim khỏi danh sách Yêu thích.";
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
