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
        public ActionResult Watch(int id)
        {
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

        public ActionResult Create([Bind(Include = "EpisodeId,EpisodeNumber,Title,VideoUrl,PublishedAt,ViewCount,FilmId")] Episode episode, HttpPostedFileBase VideoFile)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (VideoUploadHelper.HasFile(VideoFile) && !VideoUploadHelper.IsAllowed(VideoFile))
            {
                ModelState.AddModelError("VideoFile", "Chỉ hỗ trợ file video: " + VideoUploadHelper.AllowedExtensionsText);
            }

            if (ModelState.IsValid)
            {
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
        public ActionResult Edit([Bind(Include = "EpisodeId,EpisodeNumber,Title,VideoUrl,PublishedAt,ViewCount,FilmId")] Episode episode, HttpPostedFileBase VideoFile)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (VideoUploadHelper.HasFile(VideoFile) && !VideoUploadHelper.IsAllowed(VideoFile))
            {
                ModelState.AddModelError("VideoFile", "Chỉ hỗ trợ file video: " + VideoUploadHelper.AllowedExtensionsText);
            }

            if (ModelState.IsValid)
            {
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
            db.Episodes.Remove(episode);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int EpisodeId, string Content)
{
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
