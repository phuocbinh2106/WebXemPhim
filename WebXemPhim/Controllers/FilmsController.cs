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
            var films = db.Films
                .OrderByDescending(f => f.CreatedAt) // hoặc FilmId nếu không có ngày tạo
                .ToList();
            return View(films);
        }

        // GET: Films/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            // Nếu muốn chọn thể loại khi tạo phim
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload thumbnail như bạn đã có
                if (Thumbnail != null && Thumbnail.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(Thumbnail.FileName);
                    string path = Server.MapPath("~/Upload/Banner/" + fileName);
                    Thumbnail.SaveAs(path);
                    film.ThumbnailUrl = "/Upload/Banner/" + fileName;
                }

                // Gán thể loại (nếu có)
                if (SelectedGenreIds != null)
                {
                    film.Genres = new List<Genre>();
                    foreach (var genreId in SelectedGenreIds)
                    {
                        var genre = db.Genres.Find(genreId);
                        if (genre != null) film.Genres.Add(genre);
                    }
                }

                db.Films.Add(film);
                db.SaveChanges(); // FilmId được sinh ra ở đây

                // TỰ ĐỘNG TẠO EPISODE nếu nhập số tập > 0
                if (film.EpsisodeCount > 0)
                {
                    for (int i = 1; i <= film.EpsisodeCount; i++)
                    {
                        var episode = new Episode
                        {
                            FilmId = film.FilmId,
                            EpisodeNumber = i,
                            Title = $"Tập {i}",
                            // Nếu cần có trường VideoUrl/ReleaseDate... chỉnh thêm tại đây
                        };
                        db.Episodes.Add(episode);
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Details", new { id = film.FilmId });
            }

            // Nếu có lỗi, quay lại View, truyền lại các giá trị MultiSelectList
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View(film);
        }

        // GET: Films/Details/5
        public ActionResult Details(int? id)
        {
            var film = db.Films
                .Include("Genres")
                .Include("Episodes")
                .FirstOrDefault(f => f.FilmId == id);
            if (film == null) return HttpNotFound();
            return View(film);
        }

        // GET: Films/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Film film = db.Films.Find(id);
            db.Films.Remove(film);
            db.SaveChanges();
            return RedirectToAction("Index", "Admin");
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
