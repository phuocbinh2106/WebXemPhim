using System;
using System.Collections.Generic;
using System.Linq;

namespace WebXemPhim.Services
{
    /// <summary>Tình trạng một tập phim tại một thời điểm.</summary>
    public enum EpisodeAvailability
    {
        /// <summary>Đã phát hành: ai cũng xem được.</summary>
        Released,
        /// <summary>Còn trong vòng 24 giờ trước giờ phát hành: chỉ VIP (và Admin) xem được.</summary>
        EarlyAccess,
        /// <summary>Còn hơn 24 giờ mới phát hành: chỉ Admin xem được.</summary>
        Upcoming
    }

    public class VipPackage
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Days { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Luật nghiệp vụ VIP. Không phụ thuộc web hay database, nhận thời điểm "now" làm tham số
    /// để unit test được các mốc thời gian.
    /// </summary>
    public static class VipService
    {
        public static readonly TimeSpan EarlyAccessWindow = TimeSpan.FromHours(24);

        public const decimal MinTopUp = 10000m;
        public const decimal MaxTopUp = 5000000m;
        public const decimal TopUpStep = 1000m;

        public const string TransactionTopUp = "TopUp";
        public const string TransactionBuyVip = "BuyVip";

        public static readonly IReadOnlyList<VipPackage> Packages = new List<VipPackage>
        {
            new VipPackage { Code = "VIP1M",  Name = "VIP 1 tháng",  Days = 30,  Price = 30000m },
            new VipPackage { Code = "VIP3M",  Name = "VIP 3 tháng",  Days = 90,  Price = 79000m },
            new VipPackage { Code = "VIP12M", Name = "VIP 12 tháng", Days = 365, Price = 299000m },
        }.AsReadOnly();

        public static VipPackage FindPackage(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return Packages.FirstOrDefault(p => string.Equals(p.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Trả về thông báo lỗi, hoặc null nếu số tiền nạp hợp lệ.</summary>
        public static string ValidateTopUp(decimal amount)
        {
            if (amount < MinTopUp)
                return "Số tiền nạp tối thiểu là " + FormatMoney(MinTopUp) + ".";
            if (amount > MaxTopUp)
                return "Số tiền nạp tối đa là " + FormatMoney(MaxTopUp) + " mỗi lần.";
            if (amount % TopUpStep != 0)
                return "Số tiền nạp phải là bội số của " + FormatMoney(TopUpStep) + ".";
            return null;
        }

        public static bool IsVip(DateTime? vipExpiresAt, DateTime now)
        {
            return vipExpiresAt.HasValue && vipExpiresAt.Value > now;
        }

        public static EpisodeAvailability GetAvailability(DateTime? publishedAt, DateTime now)
        {
            if (!publishedAt.HasValue || publishedAt.Value <= now)
                return EpisodeAvailability.Released;
            if (publishedAt.Value <= now + EarlyAccessWindow)
                return EpisodeAvailability.EarlyAccess;
            return EpisodeAvailability.Upcoming;
        }

        public static bool CanWatch(EpisodeAvailability availability, bool isVip, bool isAdmin)
        {
            if (isAdmin) return true;
            switch (availability)
            {
                case EpisodeAvailability.Released: return true;
                case EpisodeAvailability.EarlyAccess: return isVip;
                default: return false;
            }
        }

        /// <summary>Còn VIP thì cộng dồn từ hạn hiện tại, hết hạn hoặc chưa mua thì tính từ bây giờ.</summary>
        public static DateTime CalculateNewExpiry(DateTime? currentExpiry, int days, DateTime now)
        {
            var start = IsVip(currentExpiry, now) ? currentExpiry.Value : now;
            return start.AddDays(days);
        }

        /// <summary>Trả về thông báo lỗi, hoặc null nếu mua được.</summary>
        public static string ValidatePurchase(decimal balance, VipPackage package)
        {
            if (package == null)
                return "Gói VIP không tồn tại.";
            if (balance < package.Price)
                return "Số dư không đủ. Bạn cần thêm " + FormatMoney(package.Price - balance) + ".";
            return null;
        }

        public static string FormatMoney(decimal amount)
        {
            return amount.ToString("#,##0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
        }
    }
}
