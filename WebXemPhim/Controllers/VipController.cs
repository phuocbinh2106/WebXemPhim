using System;
using System.Linq;
using System.Web.Mvc;
using WebXemPhim.Models;
using WebXemPhim.Services;

namespace WebXemPhim.Controllers
{
    /// <summary>Trang VIP: xem số dư, nạp tiền giả lập, mua gói VIP, lịch sử giao dịch.</summary>
    public class VipController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private User CurrentUser()
        {
            int userId;
            if (Session["UserId"] == null || !int.TryParse(Session["UserId"].ToString(), out userId))
                return null;
            return db.Users.Find(userId);
        }

        // GET: Vip
        public ActionResult Index()
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.IsVip = VipService.IsVip(user.VipExpiresAt, DateTime.Now);
            ViewBag.Packages = VipService.Packages;
            ViewBag.Transactions = db.VipTransactions
                .Where(t => t.UserId == user.UserId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .ToList();
            return View(user);
        }

        // POST: Vip/TopUp  (nạp tiền giả lập, không qua cổng thanh toán)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TopUp(decimal? amount)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            if (!amount.HasValue)
            {
                TempData["VipError"] = "Vui lòng nhập số tiền cần nạp.";
                return RedirectToAction("Index");
            }

            var error = VipService.ValidateTopUp(amount.Value);
            if (error != null)
            {
                TempData["VipError"] = error;
                return RedirectToAction("Index");
            }

            user.Balance += amount.Value;
            db.VipTransactions.Add(new VipTransaction
            {
                UserId = user.UserId,
                Amount = amount.Value,
                Type = VipService.TransactionTopUp,
                Description = "Nạp tiền " + VipService.FormatMoney(amount.Value),
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            TempData["VipSuccess"] = "Nạp thành công " + VipService.FormatMoney(amount.Value)
                + ". Số dư hiện tại: " + VipService.FormatMoney(user.Balance) + ".";
            return RedirectToAction("Index");
        }

        // POST: Vip/Buy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Buy(string packageCode)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var package = VipService.FindPackage(packageCode);
            var error = VipService.ValidatePurchase(user.Balance, package);
            if (error != null)
            {
                TempData["VipError"] = error;
                return RedirectToAction("Index");
            }

            var now = DateTime.Now;
            user.Balance -= package.Price;
            user.VipExpiresAt = VipService.CalculateNewExpiry(user.VipExpiresAt, package.Days, now);
            db.VipTransactions.Add(new VipTransaction
            {
                UserId = user.UserId,
                Amount = -package.Price,
                Type = VipService.TransactionBuyVip,
                Description = "Mua gói " + package.Name,
                CreatedAt = now
            });
            db.SaveChanges();

            TempData["VipSuccess"] = "Mua " + package.Name + " thành công. VIP có hạn đến "
                + user.VipExpiresAt.Value.ToString("HH:mm dd/MM/yyyy") + ".";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
