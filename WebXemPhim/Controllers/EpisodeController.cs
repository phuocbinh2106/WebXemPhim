using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;

namespace WebXemPhim.Controllers
{
    public class EpisodeController : Controller
    {
        private AppDbContext db = new AppDbContext();
        public ActionResult Watch(int id)
    {

            var episode = db.Episodes
                .Include(e => e.Film)
                .Include(e => e.Film.Episodes) // cho sidebar chuyển tập
                .FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null) return HttpNotFound();
            return View(episode); // Truyền DUY NHẤT 1 tập cho View

        }
        public ActionResult Create()
        {
            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title");
            return View();
        }
        // GET: Episode/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Episode model, HttpPostedFileBase VideoFile)
        {
            // Kiểm tra Valid
            if (ModelState.IsValid)
            {
                // Nếu có file upload và có dữ liệu
                if (VideoFile != null && VideoFile.ContentLength > 0)
                {
                    // Tạo đường dẫn lưu file
                    string fileName = Path.GetFileName(VideoFile.FileName);
                    string folderPath = Server.MapPath("~/Upload/Videos");
                    // Nếu thư mục chưa tồn tại thì tạo mới
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string savePath = Path.Combine(folderPath, fileName);
                    VideoFile.SaveAs(savePath);

                    // Lưu đường dẫn virtual vào trường VideoUrl
                    model.VideoUrl = "/Upload/Videos/" + fileName;
                }
                else
                {
                    // Nếu chưa upload video thì thông báo lỗi validation
                    ModelState.AddModelError("VideoFile", "Vui lòng chọn tệp video.");
                    return View(model);
                }

                db.Episodes.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FilmId = new SelectList(db.Films, "FilmId", "Title", model.FilmId);
            return View(model);
        }
        public ActionResult Index()
        {
            var episodes = db.Episodes.Include(e => e.Film).ToList();
            return View(episodes);
        }






    }
}