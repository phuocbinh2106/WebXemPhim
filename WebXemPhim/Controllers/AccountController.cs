using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebXemPhim.Models;

namespace WebXemPhim.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Register() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid && !db.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
            {
                db.Users.Add(model);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.Error = "Username/Email đã tồn tại";
            return View(model);
        }

        public ActionResult Login() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Username, string Password, bool RememberMe = false)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.Username, RememberMe);
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Avatar"] = user.Avatar;
                Session["Role"] = user.Role;
                Session["Nickname"] = user.Nickname;
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Sai tên hoặc mật khẩu";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult Profile()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User model, HttpPostedFileBase Avatar)
        {
            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            if (Avatar != null && Avatar.ContentLength > 0)
            {
                string fileName = Path.GetFileName(Avatar.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Upload/Avatar"), fileName);
                Avatar.SaveAs(filePath);
                user.Avatar = "/Upload/Avatar/" + fileName;
                Session["Avatar"] = user.Avatar;
            }

            user.Username = model.Username;
            user.Email = model.Email;
            db.SaveChanges();
            ViewBag.Success = "Cập nhật thành công!";
            return View(user);
        }

        // Lịch sử xem (ví dụ trả về danh sách WatchHistory)
        public ActionResult History()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var history = db.WatchHistories.Where(h => h.UserId == userId).OrderByDescending(h => h.WatchedAt).ToList();
            return View(history);
        }
        // Danh sách phim yêu thích
        public ActionResult Favourite()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var favs = db.Favourites.Where(f => f.UserId == userId).OrderByDescending(f => f.FilmId).ToList();
            return View(favs);
        }
        [HttpPost]
        public ActionResult UpdateAvatar(HttpPostedFileBase avatar)
        {
            if (avatar != null && avatar.ContentLength > 0)
            {
                var userId = (int)Session["UserId"];
                var user = db.Users.Find(userId);
                string fileName = Path.GetFileName(avatar.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Upload/Avatar"), fileName);
                avatar.SaveAs(filePath);
                user.Avatar = "/Upload/Avatar/" + fileName;
                db.SaveChanges();
                Session["Avatar"] = user.Avatar;
            }
            return RedirectToAction("Profile");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileUpdate(User model, string EditField, HttpPostedFileBase Avatar)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            // Cập nhật ảnh đại diện nếu có upload
            if (Avatar != null && Avatar.ContentLength > 0)
            {
                string fileName = Path.GetFileName(Avatar.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Upload/Avatar"), fileName);
                Avatar.SaveAs(filePath);
                user.Avatar = "/Upload/Avatar/" + fileName;
                Session["Avatar"] = user.Avatar;
            }

            // Xác định trường được chỉnh sửa hoặc đồng thời cập nhật
            if (EditField == "Nickname")
            {
                user.Nickname = model.Nickname;
                Session["Nickname"] = user.Nickname;
            }

            else if (EditField == "Username")
            {
                user.Username = model.Username;
                Session["Username"] = user.Username;
            }
            else if (EditField == "Email")
                user.Email = model.Email;

            else if (EditField == "AboutMe")
                user.AboutMe = model.AboutMe;

            else if (EditField == "Role")
                user.Role = model.Role;

            else if (EditField == "CreatedAt")
                user.CreatedAt = model.CreatedAt;

            else // Nếu submit "Lưu tất cả"
            {
                user.Nickname = model.Nickname;
                user.Username = model.Username;
                user.Email = model.Email;
                user.AboutMe = model.AboutMe;
                Session["Username"] = user.Username;     // luôn cập nhật session khi đổi tên!
            }

            db.SaveChanges();
            ViewBag.Success = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }

    }
}