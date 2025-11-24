using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
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
            // Nếu muốn chọn thể loại khi tạo phim
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (ModelState.IsValid)
            {
                if (Thumbnail != null && Thumbnail.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(Thumbnail.FileName);
                    string path = Server.MapPath("~/Upload/Banner" + fileName);
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

        // GET: Films/Details/5
        public ActionResult Details(int? id)
        {
            var film = db.Films
                .Include("Genres")
                .Include("Episodes")
                .Include("Ratings")
                .FirstOrDefault(f => f.FilmId == id);
            if (film == null) return HttpNotFound();

            var vm = new FilmDetailsViewModel
            {
                Film = film,
                Episodes = film.Episodes.OrderByDescending(e => e.EpisodeNumber).ToList(),
                // Có thể bổ sung comments nếu có
            };

            return View(vm); // Tìm file Details.cshtml ở Views/Film
        }
        
        // GET: Films/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FilmId,Title,OriginalTitle,Description,ReleaseYear,IsMovie,IsCompleted,EpsisodeCount,ThumbnailUrl,CreatedAt,UpdateAt,ViewCount,AverageRating")] Film film)
        {
            if (ModelState.IsValid)
            {
                db.Entry(film).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(film);
        }

        // GET: Films/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Films/Delete/5
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
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
