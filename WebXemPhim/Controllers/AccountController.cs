using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebXemPhim.Helpers;
using WebXemPhim.Models;
using WebXemPhim.Services;

namespace WebXemPhim.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private const string AvatarFolder = "~/Upload/Avatar/";

        public ActionResult Register() => View();

        // Release 2: chỉ nhận Username, Email, Password từ form (SCRUM-5), kiểm tra từng trường
        // với thông báo riêng (SCRUM-12, SCRUM-14), cắt khoảng trắng (SCRUM-19), băm mật khẩu (SCRUM-7).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // ký tự "<" được kiểm tra bằng luật bên dưới thay vì trang lỗi 500 (SCRUM-13)
        public ActionResult Register(string Username, string Email, string Password)
        {
            var username = AccountRules.Normalize(Username);
            var email = AccountRules.Normalize(Email);

            var errors = AccountRules.ValidateRegistration(username, email, Password);
            if (!errors.ContainsKey("Username") && db.Users.Any(u => u.Username == username))
                errors["Username"] = "Tên đăng nhập đã tồn tại.";
            if (!errors.ContainsKey("Email") && db.Users.Any(u => u.Email == email))
                errors["Email"] = "Email đã được sử dụng.";

            if (errors.Count > 0)
            {
                foreach (var e in errors) ModelState.AddModelError(e.Key, e.Value);
                ViewBag.Error = string.Join(" ", errors.Values);
                ViewBag.Username = username;
                ViewBag.Email = email;
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Password = PasswordHasher.Hash(Password),
                Role = "User",
                CreatedAt = DateTime.Now,
                // Không cho phép tự gán số dư / hạn VIP khi đăng ký (chống over-posting)
                Balance = 0,
                VipExpiresAt = null
            };
            db.Users.Add(user);
            db.SaveChanges();
            TempData["Success"] = "Đăng ký thành công, mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Success = TempData["Success"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Username, string Password, bool RememberMe = false, string returnUrl = null)
        {
            var username = AccountRules.Normalize(Username);
            var user = string.IsNullOrEmpty(username) ? null : db.Users.FirstOrDefault(u => u.Username == username);

            bool needsUpgrade;
            // So sánh trong C# để phân biệt hoa/thường (SCRUM-7); collation SQL không phân biệt.
            if (user != null && string.Equals(user.Username, username, StringComparison.Ordinal)
                && PasswordHasher.Verify(Password, user.Password, out needsUpgrade))
            {
                if (needsUpgrade)
                {
                    user.Password = PasswordHasher.Hash(Password);
                    db.SaveChanges();
                }

                // RememberMe = true tạo cookie lâu dài, Session được khôi phục lại trong Global.asax (SCRUM-15)
                FormsAuthentication.SetAuthCookie(user.Username, RememberMe);
                AuthHelper.FillSession(Session, user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Sai tên hoặc mật khẩu";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // SCRUM-8: xóa cả Session lẫn cookie .ASPXAUTH
        public ActionResult Logout()
        {
            AuthHelper.SignOut(HttpContext);
            return RedirectToAction("Login");
        }

        public new ActionResult Profile()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            if (user == null)
            {
                AuthHelper.SignOut(HttpContext);
                return RedirectToAction("Login");
            }
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(string Username, string Email, HttpPostedFileBase Avatar)
        {
            return ProfileUpdate(null, Username, Email, null, null, Avatar);
        }

        // Lịch sử xem
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

        // SCRUM-17: khách bị chuyển về Đăng nhập thay vì lỗi 500; SCRUM-11: chỉ nhận file ảnh
        [HttpPost]
        public ActionResult UpdateAvatar(HttpPostedFileBase avatar)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            var user = db.Users.Find((int)Session["UserId"]);
            if (user == null) return RedirectToAction("Login");

            if (ImageUploadHelper.HasFile(avatar))
            {
                if (!ImageUploadHelper.IsAllowed(avatar))
                {
                    TempData["Error"] = ImageUploadHelper.InvalidMessage;
                    return RedirectToAction("Profile");
                }
                user.Avatar = ImageUploadHelper.Save(avatar, Server, AvatarFolder);
                db.SaveChanges();
                Session["Avatar"] = user.Avatar;
                TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
            }
            return RedirectToAction("Profile");
        }

        // Release 2: chỉ nhận các trường được phép sửa (không còn Role, CreatedAt - SCRUM-5, SCRUM-16),
        // kiểm tra trùng Tên đăng nhập/Email (SCRUM-10), chỉ nhận file ảnh (SCRUM-11).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ProfileUpdate(string EditField, string Username, string Email, string Nickname, string AboutMe, HttpPostedFileBase Avatar)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            if (user == null) return RedirectToAction("Login");

            bool all = string.IsNullOrEmpty(EditField);
            bool editUsername = all || EditField == "Username";
            bool editEmail = all || EditField == "Email";
            bool editNickname = all || EditField == "Nickname";
            bool editAboutMe = all || EditField == "AboutMe";

            var errors = new List<string>();

            if (ImageUploadHelper.HasFile(Avatar) && !ImageUploadHelper.IsAllowed(Avatar))
                errors.Add(ImageUploadHelper.InvalidMessage);

            var username = AccountRules.Normalize(Username);
            if (editUsername)
            {
                var err = AccountRules.ValidateUsername(username);
                if (err == null && db.Users.Any(u => u.UserId != userId && u.Username == username))
                    err = "Tên đăng nhập đã tồn tại.";
                if (err != null) errors.Add(err);
            }

            var email = AccountRules.Normalize(Email);
            if (editEmail)
            {
                var err = AccountRules.ValidateEmail(email);
                if (err == null && db.Users.Any(u => u.UserId != userId && u.Email == email))
                    err = "Email đã được sử dụng.";
                if (err != null) errors.Add(err);
            }

            if (errors.Count > 0)
            {
                TempData["Error"] = string.Join(" ", errors);
                return RedirectToAction("Profile");
            }

            if (ImageUploadHelper.HasFile(Avatar))
            {
                user.Avatar = ImageUploadHelper.Save(Avatar, Server, AvatarFolder);
                Session["Avatar"] = user.Avatar;
            }

            if (editUsername)
            {
                bool changed = user.Username != username;
                user.Username = username;
                if (changed && User != null && User.Identity.IsAuthenticated)
                {
                    // Cookie xác thực đang giữ tên cũ: cấp lại theo tên mới
                    var oldCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    bool persistent = false;
                    try
                    {
                        var ticket = oldCookie == null ? null : FormsAuthentication.Decrypt(oldCookie.Value);
                        persistent = ticket != null && ticket.IsPersistent;
                    }
                    catch (ArgumentException) { }
                    FormsAuthentication.SetAuthCookie(username, persistent);
                }
            }
            if (editEmail) user.Email = email;
            if (editNickname) user.Nickname = string.IsNullOrWhiteSpace(Nickname) ? null : Nickname.Trim();
            if (editAboutMe) user.AboutMe = AboutMe;

            db.SaveChanges();
            AuthHelper.FillSession(Session, user);
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
