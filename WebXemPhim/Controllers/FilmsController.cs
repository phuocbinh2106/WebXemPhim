using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebXemPhim.Models;
using System.Collections.Generic;
using System.IO;
using WebXemPhim.Helpers;
using WebXemPhim.Services;

namespace WebXemPhim.Controllers
{
    public class FilmsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private const int PageSize = 8; // mỗi trang 8 phim, 4 phim 1 hàng × 2 hàng
        private const string BannerFolder = "~/Upload/Banner/";

        private bool IsAdmin
        {
            get { return Session["Role"]?.ToString().ToLower() == "admin"; }
        }

        /// <summary>
        /// Phân trang dùng chung. Số trang được đưa về khoảng hợp lệ (SCRUM-30) và các link phân trang
        /// giữ nguyên action + điều kiện lọc/từ khóa (SCRUM-29).
        /// </summary>
        private List<Film> Paginate(IQueryable<Film> films, int page, string action, object routeValues)
        {
            int current = SetupPaging(films.Count(), page, action, routeValues);
            return films.OrderByDescending(f => f.CreatedAt)
                .Skip((current - 1) * PageSize).Take(PageSize).ToList();
        }

        private List<Film> Paginate(List<Film> ordered, int page, string action, object routeValues)
        {
            int current = SetupPaging(ordered.Count, page, action, routeValues);
            return ordered.Skip((current - 1) * PageSize).Take(PageSize).ToList();
        }

        private int SetupPaging(int totalFilms, int page, string action, object routeValues)
        {
            int totalPages = ViewRules.TotalPages(totalFilms, PageSize);
            page = ViewRules.ClampPage(page, totalPages);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageAction = action;
            ViewBag.PageRouteValues = new System.Web.Routing.RouteValueDictionary(routeValues ?? new { });
            return page;
        }

        public ActionResult Filter(int? genreId, bool? isMovie, bool? isOngoing, int page = 1)
        {
            var films = db.Films.Include(f => f.Genres).AsQueryable();
            ViewBag.PageTitle = "📽 Danh Sách Phim";

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

            // Lọc theo thể loại
            if (genreId.HasValue)
            {
                films = films.Where(f => f.Genres.Any(g => g.GenreId == genreId.Value));

                var genre = db.Genres.Find(genreId.Value);
                if (genre != null)
                    ViewBag.PageTitle = "🎭 Thể Loại: " + genre.GenreName;
            }

            var list = Paginate(films, page, "Filter", new { genreId, isMovie, isOngoing });
            return View("Index", list);
        }

        // SCRUM-34: tìm không dấu / không phân biệt hoa thường; SCRUM-29: phân trang giữ từ khóa;
        // SCRUM-13: từ khóa chứa "<" được hiển thị dạng văn bản thay vì trang lỗi 500.
        [ValidateInput(false)]
        public ActionResult Search(string query, int page = 1)
        {
            query = (query ?? string.Empty).Trim();
            ViewBag.PageTitle = "Kết quả: " + query;
            ViewBag.Query = query;

            var matched = db.Films
                .OrderByDescending(f => f.CreatedAt)
                .ToList()
                .Where(f => ViewRules.MatchesSearch(query, f.Title, f.OriginalTitle))
                .ToList();

            var list = Paginate(matched, page, "Search", new { query });
            if (matched.Count == 0)
                ViewBag.EmptyMessage = string.IsNullOrEmpty(query) ? "Vui lòng nhập từ khóa tìm kiếm." : "Không tìm thấy phim phù hợp.";
            return View("Index", list);
        }

        // GET: Films
        public ActionResult Index(string type, int page = 1)
        {
            IQueryable<Film> films = db.Films;

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

            var list = Paginate(films, page, "Index", string.IsNullOrEmpty(type) ? null : new { type });
            return View(list);
        }

        // GET: Films/Create
        public ActionResult Create()
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName");
            return View();
        }

        /// <summary>Kiểm tra dữ liệu phim dùng cho cả Tạo và Sửa (SCRUM-22, SCRUM-24, SCRUM-25).</summary>
        private void ValidateFilm(Film film, HttpPostedFileBase thumbnail)
        {
            if (!string.IsNullOrWhiteSpace(film.Title))
            {
                var title = film.Title.Trim();
                bool duplicate = db.Films
                    .Where(f => f.FilmId != film.FilmId)
                    .Select(f => f.Title)
                    .ToList()
                    .Any(t => FilmRules.SameName(t, title));
                if (duplicate)
                    ModelState.AddModelError("Title", "Phim \"" + title + "\" đã tồn tại.");
            }

            var yearError = FilmRules.ValidateReleaseYear(film.ReleaseYear, DateTime.Now.Year);
            if (yearError != null) ModelState.AddModelError("ReleaseYear", yearError);

            var countError = FilmRules.ValidateEpisodeCount(film.EpsisodeCount);
            if (countError != null) ModelState.AddModelError("EpsisodeCount", countError);

            if (ImageUploadHelper.HasFile(thumbnail) && !ImageUploadHelper.IsAllowed(thumbnail))
                ModelState.AddModelError("Thumbnail", ImageUploadHelper.InvalidMessage);
        }

        // POST: Films/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // tên phim chứa "<" được lưu và hiển thị dạng văn bản (SCRUM-13)
        public ActionResult Create(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail, HttpPostedFileBase VideoFile)
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");

            ValidateFilm(film, Thumbnail);
            if (VideoUploadHelper.HasFile(VideoFile) && !VideoUploadHelper.IsAllowed(VideoFile))
            {
                ModelState.AddModelError("VideoFile", "Chỉ hỗ trợ file video: " + VideoUploadHelper.AllowedExtensionsText);
            }

            if (ModelState.IsValid)
            {
                film.Title = film.Title.Trim();
                // Không cho client tự đặt các trường hệ thống
                film.CreatedAt = DateTime.Now;
                film.UpdateAt = null;
                film.ViewCount = 0;
                film.AverageRating = 0;

                // Có chọn video thì cần ít nhất 1 tập để gắn video vào
                string videoUrl = null;
                if (VideoUploadHelper.HasFile(VideoFile))
                {
                    videoUrl = VideoUploadHelper.Save(VideoFile, Server);
                    if (!(film.EpsisodeCount > 0))
                        film.EpsisodeCount = 1;
                }

                // Ảnh nền: luôn lưu với tên mới để không ghi đè ảnh phim khác (SCRUM-23)
                film.ThumbnailUrl = ImageUploadHelper.HasFile(Thumbnail)
                    ? ImageUploadHelper.Save(Thumbnail, Server, BannerFolder)
                    : null;

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
                foreach (var number in FilmRules.MissingEpisodeNumbers(Enumerable.Empty<int>(), film.EpsisodeCount))
                {
                    db.Episodes.Add(new Episode
                    {
                        FilmId = film.FilmId,
                        EpisodeNumber = number,
                        Title = $"Tập {number}",
                        VideoUrl = number == 1 ? videoUrl : null,
                    });
                }
                db.SaveChanges();

                return RedirectToAction("Details", new { id = film.FilmId });
            }

            // Nếu có lỗi, quay lại View, truyền lại các giá trị MultiSelectList
            ViewBag.SelectedGenreIds = new MultiSelectList(db.Genres, "GenreId", "GenreName", SelectedGenreIds ?? new int[0]);
            return View(film);
        }

        // GET: Films/Details
        public ActionResult Details(int? id)
        {
            if (id == null) return HttpNotFound();
            var film = db.Films
                .Include("Genres")
                .Include("Episodes")
                .FirstOrDefault(f => f.FilmId == id);
            if (film == null) return HttpNotFound();
            var ratings = db.Ratings.Where(r => r.FilmId == film.FilmId).Include(r => r.User).ToList();
            ViewBag.AvgRating = ViewRules.Average(ratings.Select(r => r.Value));
            ViewBag.Ratings = ratings;
            ViewBag.RatingError = TempData["RatingError"];
            ViewBag.FavouriteMessage = TempData["FavouriteMessage"];

            // Trạng thái Yêu thích của người đang xem (để hiện nút Bỏ yêu thích - SCRUM-32)
            int userId;
            ViewBag.IsFavourite = Session["UserId"] != null
                && int.TryParse(Session["UserId"].ToString(), out userId)
                && db.Favourites.Any(f => f.UserId == userId && f.FilmId == film.FilmId);
            return View(film);
        }

        // GET: Films/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

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
        // SCRUM-20: chỉ cập nhật các trường có trên form; ảnh nền, Ngày tạo, Lượt xem, Điểm TB giữ nguyên.
        // SCRUM-27: tăng Số tập thì tự tạo thêm các tập còn thiếu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Film film, int[] SelectedGenreIds, HttpPostedFileBase Thumbnail)
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");

            var existingFilm = db.Films.Include("Genres").Include("Episodes").FirstOrDefault(f => f.FilmId == film.FilmId);
            if (existingFilm == null)
                return HttpNotFound();

            ValidateFilm(film, Thumbnail);

            if (ModelState.IsValid)
            {
                existingFilm.Title = film.Title.Trim();
                existingFilm.OriginalTitle = film.OriginalTitle;
                existingFilm.Description = film.Description;
                existingFilm.ReleaseYear = film.ReleaseYear;
                existingFilm.IsMovie = film.IsMovie;
                existingFilm.IsCompleted = film.IsCompleted;
                existingFilm.EpsisodeCount = film.EpsisodeCount;

                if (ImageUploadHelper.HasFile(Thumbnail))
                    existingFilm.ThumbnailUrl = ImageUploadHelper.Save(Thumbnail, Server, BannerFolder);

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

                var existingNumbers = existingFilm.Episodes.Select(e => e.EpisodeNumber).ToList();
                foreach (var number in FilmRules.MissingEpisodeNumbers(existingNumbers, existingFilm.EpsisodeCount))
                {
                    db.Episodes.Add(new Episode
                    {
                        FilmId = existingFilm.FilmId,
                        EpisodeNumber = number,
                        Title = $"Tập {number}"
                    });
                }

                db.SaveChanges();
                return RedirectToAction("Index", "Admin");
            }

            // Hiển thị lại form với ảnh hiện tại
            film.ThumbnailUrl = existingFilm.ThumbnailUrl;
            var allGenres = db.Genres.ToList();
            var selectedIds = SelectedGenreIds ?? new int[] { };
            ViewBag.SelectedGenreIds = new MultiSelectList(allGenres, "GenreId", "GenreName", selectedIds);

            return View(film);
        }

        // GET: Films/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Film film = db.Films.Find(id);
            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        // POST: Films/Delete/5
        // SCRUM-6: chỉ Admin được xóa phim (trước đây khách/User gửi POST trực tiếp vẫn xóa được).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin)
                return RedirectToAction("Index", "Home");

            var film = db.Films.Find(id);
            if (film == null)
                return HttpNotFound();

            // Xóa các episode liên quan (và comment, lịch sử xem của episode đó)
            var episodes = db.Episodes.Where(e => e.FilmId == id).ToList();
            var episodeIds = episodes.Select(e => e.EpisodeId).ToList();
            db.Comments.RemoveRange(db.Comments.Where(c => c.EpisodeId.HasValue && episodeIds.Contains(c.EpisodeId.Value)));
            db.WatchHistories.RemoveRange(db.WatchHistories.Where(h => episodeIds.Contains(h.EpisodeId)));
            db.Episodes.RemoveRange(episodes);

            // Xóa các comment trực tiếp trên phim
            db.Comments.RemoveRange(db.Comments.Where(c => c.FilmId == id));

            // Xóa các bảng liên quan khác
            db.Favourites.RemoveRange(db.Favourites.Where(f => f.FilmId == id));
            db.Ratings.RemoveRange(db.Ratings.Where(r => r.FilmId == id));
            db.Schedules.RemoveRange(db.Schedules.Where(s => s.FilmId == id));

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

        // SCRUM-28: chỉ nhận điểm 1-5, phim phải tồn tại; cập nhật lại điểm trung bình của phim.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRating(int filmId, int value)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            if (userId == 0)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", new { id = filmId }) });

            var film = db.Films.Find(filmId);
            if (film == null)
                return HttpNotFound();

            if (!ViewRules.IsValidRating(value))
            {
                TempData["RatingError"] = "Điểm đánh giá phải từ " + ViewRules.MinRating + " đến " + ViewRules.MaxRating + " sao.";
                return RedirectToAction("Details", new { id = filmId });
            }

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

            film.AverageRating = ViewRules.Average(db.Ratings.Where(r => r.FilmId == filmId).Select(r => r.Value).ToList());
            db.SaveChanges();
            return RedirectToAction("Details", new { id = filmId });
        }
    }
}
