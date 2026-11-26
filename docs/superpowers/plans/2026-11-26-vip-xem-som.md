# VIP xem phim sớm – Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Nạp tiền giả lập, mua gói VIP, VIP xem tập sớm 24h.
**Architecture:** Luật thuần trong `WebXemPhim/Services/VipService.cs` (không dùng System.Web/EF), controller gọi vào; test project MSTest link trực tiếp file nguồn của Services.
**Tech Stack:** ASP.NET MVC 5, EF6, .NET Framework 4.8.1, MSTest.
**Spec:** `docs/superpowers/specs/2026-11-26-vip-xem-som-design.md`

## Global Constraints
- Giá/ngày gói: VIP1M 30 ngày 30000; VIP3M 90 ngày 79000; VIP12M 365 ngày 299000.
- Nạp: 10000..5000000, bội số 1000. Cửa sổ xem sớm: đúng 24 giờ.
- Thông báo tiếng Việt.

## Review Focus
- Mốc biên: PublishedAt == now (Released), == now+24h (EarlyAccess), == now+24h+1 tick (Upcoming) → test trong Task 1.
- VipExpiresAt == now → không còn VIP → test Task 1.
- Mua gói khi đã hết hạn → tính từ now, không cộng dồn từ hạn cũ → test Task 1.
- Số dư đúng bằng giá → mua được, số dư về 0 → test Task 1.
- Mã gói không tồn tại / số tiền âm, 0, lẻ → lỗi → test Task 1.

---

### Task 1: VipService + unit test
**Files:** Create `WebXemPhim/Services/VipService.cs`, `WebXemPhim.Tests/WebXemPhim.Tests.csproj`, `WebXemPhim.Tests/VipServiceTests.cs`; Modify `WebXemPhim.sln`, `WebXemPhim/WebXemPhim.csproj`.
**Produces:**
- `enum EpisodeAvailability { Released, EarlyAccess, Upcoming }`
- `class VipPackage { string Code; string Name; int Days; decimal Price; }`
- `static class VipService`: `EarlyAccessWindow` (TimeSpan 24h), `MinTopUp`, `MaxTopUp`, `TopUpStep`, `Packages` (IReadOnlyList<VipPackage>), `FindPackage(string code)`, `ValidateTopUp(decimal amount) -> string` (null = hợp lệ), `IsVip(DateTime? expiresAt, DateTime now)`, `GetAvailability(DateTime? publishedAt, DateTime now)`, `CanWatch(EpisodeAvailability, bool isVip, bool isAdmin)`, `CalculateNewExpiry(DateTime? currentExpiry, int days, DateTime now)`, `ValidatePurchase(decimal balance, VipPackage pkg) -> string`.
- [ ] Viết test (các mốc trong Review Focus), sau đó code, commit.

### Task 2: Model + DbContext
**Files:** Modify `Models/User.cs` (Balance, VipExpiresAt, VipTransactions nav), Create `Models/VipTransaction.cs`, Modify `Models/AppDbContext.cs` (DbSet + quan hệ cascade), `Controllers/AccountController.cs` (Register reset Balance/VipExpiresAt chống over-posting). Dữ liệu mẫu (user VIP, tập phát hành tương lai) để thread tạo dữ liệu mẫu lo, tránh sửa trùng `Configuration.cs`.

### Task 3: VipController + view
**Files:** Create `Controllers/VipController.cs`, `Views/Vip/Index.cshtml`; Modify `_Layout.cshtml` (mục "Gói VIP" + nhãn VIP), `Views/Account/Profile.cshtml` (số dư, hạn VIP).
- TopUp/Buy: POST + AntiForgery, chưa đăng nhập → Login; ghi `VipTransaction`; TempData thông báo.

### Task 4: Chặn xem + nhãn tập
**Files:** Modify `Controllers/EpisodesController.cs` (Watch), Create `Views/Episodes/Locked.cshtml`, Modify `Views/Episodes/Watch.cshtml`, `Views/Films/Details.cshtml` (tự đọc Role từ Session, không sửa FilmsController để tránh đụng thread chọn file).
