using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebXemPhim.Models;

namespace WebXemPhim.Controllers
{
    public class FilmsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Films
        public ActionResult Index()
        {
            var latestFilms = db.Films
                    .OrderByDescending(f => f.CreatedAt) // sắp xếp theo ngày tạo
                    .Take(4) // chỉ 4 phim
                    .ToList();
            return View(latestFilms);
        }
        public ActionResult MoreFilms(int page = 1, int pageSize = 8)
        {
            var films = db.Films
                          .OrderByDescending(f => f.CreatedAt)
                          .Skip((page - 1) * pageSize) // bỏ qua các phim cũ
                          .Take(pageSize) // lấy số phim theo trang
                          .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalFilms = db.Films.Count();

            return View(films);
        }

        // GET: Films/Create
        public ActionResult Create()
        {
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View();
        }

        // POST: Films/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (ModelState.IsValid)
            {
                if (Thumbnail != null && Thumbnail.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(Thumbnail.FileName);
                    string path = Server.MapPath("~/Upload/Banner/" + fileName);
                    Thumbnail.SaveAs(path);

                    film.ThumbnailUrl = "/Upload/Banner/" + fileName;
                }

                db.Films.Add(film);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu có lỗi thì quay lại view với model hiện tại
            // Đồng thời cần truyền lại MultiSelectList nếu có dùng chọn thể loại nhiều
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View(film);
        }

        // GET: Films/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }
            return View(film);
        }

        // GET: Films/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Film film = db.Films.Find(id);
            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        // POST: Films/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Film film, HttpPostedFileBase Thumbnail)
        {
            if (ModelState.IsValid)
            {
                // Nếu có đổi ảnh mới
                if (Thumbnail != null && Thumbnail.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(Thumbnail.FileName);
                    string path = Server.MapPath("~/Upload/Banner/" + fileName);
                    Thumbnail.SaveAs(path);

                    film.ThumbnailUrl = "/Upload/Banner/" + fileName;
                }

                film.UpdateAt = DateTime.Now;

                db.Entry(film).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(film);
        }

        // GET: Films/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Film film = db.Films.Find(id);
            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        // POST: Films/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Film film = db.Films.Find(id);
            db.Films.Remove(film);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
