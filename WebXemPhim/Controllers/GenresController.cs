using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebXemPhim.Models;
using WebXemPhim.Services;

namespace WebXemPhim.Controllers
{
    public class GenresController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public PartialViewResult GenreDropdown()
        {
            var genres = db.Genres.ToList();
            return PartialView("_GenreDropdown", genres);
        }
        // GET: Genres
        // SCRUM-9: trang quản lý thể loại chỉ dành cho Admin
        public ActionResult Index()
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            return View(db.Genres.ToList());
        }

        // GET: Genres/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                return HttpNotFound();
            }
            return View(genre);
        }

        // GET: Genres/Create
        public ActionResult Create()
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Genres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "GenreId,GenreName")] Genre genre)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            ValidateGenre(genre);
            if (ModelState.IsValid)
            {
                genre.GenreName = genre.GenreName.Trim();
                db.Genres.Add(genre);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(genre);
        }

        // GET: Genres/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                return HttpNotFound();
            }
            return View(genre);
        }

        // POST: Genres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "GenreId,GenreName")] Genre genre)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            ValidateGenre(genre);
            if (ModelState.IsValid)
            {
                genre.GenreName = genre.GenreName.Trim();
                db.Entry(genre).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(genre);
        }

        // GET: Genres/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                return HttpNotFound();
            }
            return View(genre);
        }

        // POST: Genres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["Role"]?.ToString().ToLower() != "admin")
                return RedirectToAction("Index", "Home");
            Genre genre = db.Genres.Find(id);
            if (genre == null) return HttpNotFound();
            db.Genres.Remove(genre);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // SCRUM-25, SCRUM-26: tên bắt buộc, tối đa 50 ký tự, không trùng thể loại khác
        private void ValidateGenre(Genre genre)
        {
            var error = FilmRules.ValidateGenreName(genre.GenreName);
            if (error == null)
            {
                var name = genre.GenreName.Trim();
                bool duplicate = db.Genres
                    .Where(g => g.GenreId != genre.GenreId)
                    .Select(g => g.GenreName)
                    .ToList()
                    .Any(n => FilmRules.SameName(n, name));
                if (duplicate) error = "Thể loại đã tồn tại.";
            }
            if (error != null) ModelState.AddModelError("GenreName", error);
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
