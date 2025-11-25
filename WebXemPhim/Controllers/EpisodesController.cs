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

namespace WebXemPhim.Controllers
{
    public class EpisodesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Episodes
        public ActionResult Index(int? filmId)
        {
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
            // id là EpisodeId
            var episode = db.Episodes.Include("Film").FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null) return HttpNotFound();
            return View(episode); // View này để trình chiếu video
        }

        public ActionResult Watch(int id)
        {
            var episode = db.Episodes.Include("Film").FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null) return HttpNotFound();
            return View(episode);
        }
        // GET: Episodes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Episode episode = db.Episodes.Find(id);
            if (episode == null) return HttpNotFound();
            return View(episode);
        }

        // GET: Episodes/Create
        public ActionResult Create()
        {
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
            if (VideoFile != null && VideoFile.ContentLength > 0)
            {
                // Đường dẫn lưu video
                var fileName = Path.GetFileName(VideoFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Upload/Videos/"), fileName);
                VideoFile.SaveAs(path);

                // Lưu đường dẫn vào Episode
                episode.VideoUrl = "/Upload/Videos/" + fileName;
            }

            if (ModelState.IsValid)
            {
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
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Episode episode = db.Episodes.Find(id);
            if (episode == null) return HttpNotFound();

            // Lấy danh sách file video trong thư mục Upload/Videos
            string videoDirPath = Server.MapPath("~/Upload/Videos/");
            var videoFileList = Directory.Exists(videoDirPath)
                ? Directory.GetFiles(videoDirPath)
                    .Select(f => new SelectListItem
                    {
                        Text = Path.GetFileName(f),
                        Value = "/Upload/Videos/" + Path.GetFileName(f)
                    }).ToList()
                : new List<SelectListItem>();

            ViewBag.VideoFiles = videoFileList;

            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title", episode.FilmId);
            return View(episode);
        }

        // POST: Episodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EpisodeId,EpisodeNumber,Title,VideoUrl,PublishedAt,ViewCount,FilmId")] Episode episode)
        {
            if (ModelState.IsValid)
            {
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
    }
}
