using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;
using System.IO;
using WebXemPhim.Services;
using WebXemPhim.Helpers;

namespace WebXemPhim.Controllers
{
    public class EpisodesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Episodes
        
        public ActionResult Index(int? filmId)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (filmId.HasValue)
            {
                var episodes = db.Episodes.Where(e => e.FilmId == filmId).ToList();
                ViewBag.Film = db.Films.Find(filmId);
                return View(episodes);
            }
            else
            {
                var episodes = db.Episodes.Include(e => e.Film).ToList();
                return View(episodes);
            }
            // ...
        }
        // SCRUM-30: id không phải số / không tồn tại -> 404 thay vì lỗi 500
        public ActionResult Watch(int? id)
        {
            if (id == null) return HttpNotFound();
            var episode = db.Episodes.Include(e => e.Film).FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null) return HttpNotFound();

            // --- Kiểm tra quyền xem sớm (VIP) ---
            var now = DateTime.Now;
            bool isAdmin = Session["Role"]?.ToString().ToLower() == "admin";
            bool isVip = false;
            int currentUserId;
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out currentUserId))
            {
                var currentUser = db.Users.Find(currentUserId);
                isVip = currentUser != null && VipService.IsVip(currentUser.VipExpiresAt, now);
            }
            ViewBag.IsVip = isVip;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Now = now;

            var availability = VipService.GetAvailability(episode.PublishedAt, now);
            if (!VipService.CanWatch(availability, isVip, isAdmin))
            {
                ViewBag.Availability = availability;
                ViewBag.IsLoggedIn = Session["UserId"] != null;
                return View("Locked", episode);
            }


            // SCRUM-31: tăng lượt xem của tập và của phim mỗi lần mở xem
            episode.ViewCount++;
            if (episode.Film != null) episode.Film.ViewCount++;
            db.SaveChanges();

            // Truy vấn tất cả comment vừa lưu (bao gồm info user, không cache cũ)
            var comments = db.Comments
                .Include(c => c.User)
                .Where(c => c.EpisodeId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            int userId = 0;
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out userId) && userId > 0)
            {
                // Kiểm tra đã có record chưa
                var history = db.WatchHistories.FirstOrDefault(h => h.UserId == userId && h.EpisodeId == id);
                if (history == null)
                {
                    history = new WatchHistory
                    {
                        UserId = userId,
                        EpisodeId = id,
                        WatchedAt = DateTime.Now
                    };
                    db.WatchHistories.Add(history);
                }
                else
                {
                    // Nếu đã có, cập nhật thời gian xem mới nhất
                    history.WatchedAt = DateTime.Now;
                }
                db.SaveChanges();
            }
            // --- KẾT THÚC phần lịch sử xem ---
            ViewBag.Comments = comments;

            return View(episode);

        }        // GET: Episodes/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Episode episode = db.Episodes.Find(id);
            if (episode == null) return HttpNotFound();
            return View(episode);
        }

        // GET: Episodes/Create

        public ActionResult Create()
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title");
            return View();
        }

        // POST: Episodes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "EpisodeId,EpisodeNumber,Title,VideoUrl,PublishedAt,ViewCount,FilmId")] Episode episode, HttpPostedFileBase VideoFile)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (VideoUploadHelper.HasFile(VideoFile) && !VideoUploadHelper.IsAllowed(VideoFile))
            {
                ModelState.AddModelError("VideoFile", "Chỉ hỗ trợ file video: " + VideoUploadHelper.AllowedExtensionsText);
            }
            ValidateEpisode(episode);

            if (ModelState.IsValid)
            {
                episode.Title = episode.Title.Trim();
                // Lưu file video người dùng chọn từ máy lên server
                if (VideoUploadHelper.HasFile(VideoFile))
                    episode.VideoUrl = VideoUploadHelper.Save(VideoFile, Server);

                db.Episodes.Add(episode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title", episode.FilmId);
            return View(episode);
        }

        // GET: Episodes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Episode episode = db.Episodes.Find(id);
            if (episode == null) return HttpNotFound();

            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title", episode.FilmId);
            return View(episode);
        }

        // POST: Episodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "EpisodeId,EpisodeNumber,Title,VideoUrl,PublishedAt,ViewCount,FilmId")] Episode episode, HttpPostedFileBase VideoFile)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (VideoUploadHelper.HasFile(VideoFile) && !VideoUploadHelper.IsAllowed(VideoFile))
            {
                ModelState.AddModelError("VideoFile", "Chỉ hỗ trợ file video: " + VideoUploadHelper.AllowedExtensionsText);
            }

            ValidateEpisode(episode);

            if (ModelState.IsValid)
            {
                episode.Title = episode.Title.Trim();
                // Chọn file mới thì thay video, không chọn thì giữ video hiện tại (VideoUrl ẩn trong form)
                if (VideoUploadHelper.HasFile(VideoFile))
                    episode.VideoUrl = VideoUploadHelper.Save(VideoFile, Server);

                db.Entry(episode).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title", episode.FilmId);
            return View(episode);
        }
        // GET: Episodes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Episode episode = db.Episodes.Find(id);
            if (episode == null)
            {
                return HttpNotFound();
            }
            return View(episode);
        }

        // POST: Episodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            Episode episode = db.Episodes.Find(id);
            if (episode == null) return HttpNotFound();

            // SCRUM-21: xóa bình luận và lịch sử xem của tập trước (khóa ngoại không xóa dây chuyền)
            db.Comments.RemoveRange(db.Comments.Where(c => c.EpisodeId == id));
            db.WatchHistories.RemoveRange(db.WatchHistories.Where(h => h.EpisodeId == id));
            db.Episodes.Remove(episode);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// SCRUM-24, SCRUM-25, SCRUM-26: số thứ tự &gt; 0 và không trùng trong cùng phim,
        /// tên tập bắt buộc, tối đa 255 ký tự, phim phải tồn tại.
        /// </summary>
        private void ValidateEpisode(Episode episode)
        {
            ModelState.Remove("EpisodeNumber"); // thay thông báo mặc định bằng thông báo tiếng Việt
            var numberError = FilmRules.ValidateEpisodeNumber(episode.EpisodeNumber);
            if (numberError != null)
                ModelState.AddModelError("EpisodeNumber", numberError);
            else if (db.Episodes.Any(e => e.FilmId == episode.FilmId && e.EpisodeNumber == episode.EpisodeNumber && e.EpisodeId != episode.EpisodeId))
                ModelState.AddModelError("EpisodeNumber", "Số thứ tự tập " + episode.EpisodeNumber + " đã tồn tại trong phim này.");

            var titleError = FilmRules.ValidateEpisodeTitle(episode.Title);
            if (titleError != null) ModelState.AddModelError("Title", titleError);

            if (!db.Films.Any(f => f.FilmId == episode.FilmId))
                ModelState.AddModelError("FilmId", "Vui lòng chọn phim.");

            if (episode.ViewCount < 0)
                ModelState.AddModelError("ViewCount", "Số lượt xem không được âm.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // SCRUM-13: bình luận chứa thẻ HTML được lưu và hiển thị dạng văn bản (Razor tự mã hóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddComment(int EpisodeId, string Content)
{
    if (!db.Episodes.Any(e => e.EpisodeId == EpisodeId))
        return HttpNotFound();
    if (Content != null && Content.Length > 2000)
        Content = Content.Substring(0, 2000);
    // Kiểm tra xác thực và nội dung
    if (Session["UserId"] == null || string.IsNullOrWhiteSpace(Content))
        return RedirectToAction("Watch", new { id = EpisodeId });
 
    int userId;
    if (int.TryParse(Session["UserId"].ToString(), out userId) && userId > 0)
    {
        var comment = new Comment
        {
            EpisodeId = EpisodeId,
            UserId = userId,
            Content = Content,
            CreatedAt = DateTime.Now
        };

        try
        {
            db.Comments.Add(comment);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Lỗi lưu comment: " + ex.Message);
        }
    }
    return RedirectToAction("Watch", new { id = EpisodeId });
}



    }
}
