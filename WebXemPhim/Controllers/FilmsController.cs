using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebXemPhim.Models;
using System.Collections.Generic;
using System.IO;

namespace WebXemPhim.Controllers
{
    public class FilmsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Filter(int? genreId, bool? isMovie, bool? isOngoing)
        {
            var films = db.Films.Include(f => f.Genres).AsQueryable();

            // Lọc phim lẻ / phim bộ
            if (isMovie.HasValue)
            {
                films = films.Where(f => f.IsMovie == isMovie.Value);
                ViewBag.PageTitle = isMovie.Value ? "🎬 Phim Lẻ" : "📺 Phim Bộ";
            }

            // Lọc phim đang chiếu
            if (isOngoing.HasValue)
            {
                films = films.Where(f => f.IsCompleted == !isOngoing.Value);
                ViewBag.PageTitle = isOngoing.Value ? "🔥 Đang Chiếu" : "✔ Trọn Bộ";
            }

            // Lọc theo thể loại (sửa lỗi GenreId)
            if (genreId.HasValue)
            {
                films = films.Where(f => f.Genres.Any(g => g.GenreId == genreId.Value));

                var genre = db.Genres.Find(genreId.Value);
                if (genre != null)
                    ViewBag.PageTitle = "🎭 Thể Loại: " + genre.GenreName;
            }

            return View("Index", films.ToList());
        }

        // GET: Films
        public ActionResult Index(string type)
        {
            var films = db.Films.AsQueryable();

            // Gán tiêu đề mặc định
            ViewBag.PageTitle = "📽 Phim Mới Cập Nhật";

            if (type == "movie")        // Phim lẻ
            {
                films = films.Where(f => f.IsMovie == true);
                ViewBag.PageTitle = "🎬 Phim Lẻ";
            }
            else if (type == "series")  // Phim bộ
            {
                films = films.Where(f => f.IsMovie == false);
                ViewBag.PageTitle = "📺 Phim Bộ";
            }
            else
            {
                films = films.OrderByDescending(f => f.CreatedAt).Take(10);
            }
            
            var filmList = films.OrderByDescending(f => f.CreatedAt).Take(10).ToList();

            return View(filmList);
        }

        // GET: Films/Create
        public ActionResult Create()
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View();
        }

        // POST: Films/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

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

        // GET: Films/Details
        public ActionResult Details(int? id)
        {
            var film = db.Films
                .Include("Genres")
                .Include("Episodes")
                .FirstOrDefault(f => f.FilmId == id);
            if (film == null) return HttpNotFound();
            var ratings = db.Ratings.Where(r => r.FilmId == film.FilmId).Include(r => r.User).ToList();
            ViewBag.AvgRating = ratings.Any() ? ratings.Average(r => r.Value) : 0;
            ViewBag.Ratings = ratings;
            return View(film);
        }

        // GET: Films/Edit/5
        public ActionResult Edit(int id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            var film = db.Films.Include("Genres").FirstOrDefault(f => f.FilmId == id);
            if (film == null)
                return HttpNotFound();

            // Truyền đa chọn thể loại cho view
            var allGenres = db.Genres.ToList();
            var selectedIds = film.Genres.Select(g => g.GenreId).ToList();
            ViewBag.SelectedGenreIds = new MultiSelectList(allGenres, "GenreId", "GenreName", selectedIds);

            return View(film);
        }
        // POST: Films/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var existingFilm = db.Films.Include("Genres").FirstOrDefault(f => f.FilmId == film.FilmId);
                if (existingFilm == null)
                    return HttpNotFound();

                db.Entry(existingFilm).CurrentValues.SetValues(film);

                if (Thumbnail != null && Thumbnail.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(Thumbnail.FileName);
                    string path = Server.MapPath("~/Upload/Banner/" + fileName);
                    Thumbnail.SaveAs(path);

                    existingFilm.ThumbnailUrl = "/Upload/Banner/" + fileName;
                }

                existingFilm.UpdateAt = DateTime.Now;

                // Reset và cập nhật lại genres
                existingFilm.Genres.Clear();
                if (SelectedGenreIds != null)
                {
                    foreach (int gid in SelectedGenreIds)
                    {
                        var genre = db.Genres.Find(gid);
                        if (genre != null)
                            existingFilm.Genres.Add(genre);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Truyền lại select list khi ModelState không hợp lệ
            var allGenres = db.Genres.ToList();
            var selectedIds = SelectedGenreIds ?? new int[] { };
            ViewBag.SelectedGenreIds = new MultiSelectList(allGenres, "GenreId", "GenreName", selectedIds);

            return View(film);
        }

        // GET: Films/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Film film = db.Films.Find(id);
            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Tìm phim cần xóa
            var film = db.Films.Find(id);

            // Xóa các episode liên quan (và các comment của episode đó)
            var episodes = db.Episodes.Where(e => e.FilmId == id).ToList();
            foreach (var epi in episodes)
            {
                var comments = db.Comments.Where(c => c.EpisodeId == epi.EpisodeId).ToList();
                db.Comments.RemoveRange(comments);
            }
            db.Episodes.RemoveRange(episodes);

            // Xóa các comment trực tiếp trên phim (nếu có loại comment này)
            var filmComments = db.Comments.Where(c => c.FilmId == id).ToList();
            db.Comments.RemoveRange(filmComments);

            // Xóa các bảng liên quan khác
            var favourites = db.Favourites.Where(f => f.FilmId == id).ToList();
            db.Favourites.RemoveRange(favourites);

            var ratings = db.Ratings.Where(r => r.FilmId == id).ToList();
            db.Ratings.RemoveRange(ratings);

            var schedules = db.Schedules.Where(s => s.FilmId == id).ToList();
            db.Schedules.RemoveRange(schedules);

            // Cuối cùng xóa phim
            db.Films.Remove(film);
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRating(int filmId, int value)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var oldRating = db.Ratings.FirstOrDefault(r => r.FilmId == filmId && r.UserId == userId);
            if (oldRating == null)
            {
                db.Ratings.Add(new Rating
                {
                    FilmId = filmId,
                    UserId = userId,
                    Value = value,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                oldRating.Value = value;
                oldRating.CreatedAt = DateTime.Now;
            }
            db.SaveChanges();
            return RedirectToAction("Details", new { id = filmId });
        }


    }
}
