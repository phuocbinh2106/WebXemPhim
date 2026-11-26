# Thiết kế: Chức năng VIP xem phim sớm

## Mục đích
Đồ án môn Kiểm định chất lượng phần mềm. Thêm chức năng VIP để có thêm nghiệp vụ đáng kiểm thử:
phân quyền VIP / thường, mốc thời gian xem sớm, hạn gói, số dư khi nạp tiền.
Người dùng đã đồng ý làm, yêu cầu giữ đơn giản, nạp tiền chỉ giả lập.

## Mục tiêu
- Nạp tiền giả lập vào số dư tài khoản.
- Mua gói VIP bằng số dư; gói có thời hạn; mua thêm khi còn hạn thì cộng dồn.
- VIP được xem tập phim sớm hơn 24 giờ so với người thường.
- Lưu lịch sử giao dịch (nạp tiền, mua VIP).
- Luật nghiệp vụ nằm trong một lớp thuần (không phụ thuộc web/DB), có unit test.

## Không làm
- Cổng thanh toán thật, hoàn tiền, khuyến mãi, admin CRUD gói VIP.
- Sửa form tạo/sửa tập (thread khác đang sửa phần chọn file).

## Ràng buộc
- ASP.NET MVC 5, .NET Framework 4.8.1, EF6 Code First có Migrations, SQL Server.
- Session lưu UserId/Role sau khi đăng nhập; trạng thái VIP đọc từ DB mỗi lần cần (không cache trong Session).

## Các cách đã cân nhắc
1. **Luật nằm trong lớp tĩnh `VipService` nhận `now` làm tham số (đã chọn).** Dễ unit test các mốc thời gian, controller chỉ gọi vào.
2. Luật viết thẳng trong controller: nhanh nhưng không unit test được, không hợp với đồ án kiểm thử.
3. Thêm cờ `IsVipOnly`/`VipReleaseAt` riêng cho từng tập: linh hoạt hơn nhưng admin phải nhập thêm, sửa nhiều form. Dùng `Episode.PublishedAt` có sẵn là đủ.

## Thiết kế
**Dữ liệu**
- `User.Balance` (decimal, mặc định 0), `User.VipExpiresAt` (DateTime?).
- Bảng mới `VipTransactions`: TransactionId, UserId (FK, cascade), Amount, Type ("TopUp" | "BuyVip"), Description, CreatedAt.

**Gói VIP (cố định trong code)**

| Mã | Tên | Số ngày | Giá |
|----|-----|---------|-----|
| VIP1M | VIP 1 tháng | 30 | 30.000đ |
| VIP3M | VIP 3 tháng | 90 | 79.000đ |
| VIP12M | VIP 12 tháng | 365 | 299.000đ |

**Luật**
- Nạp tiền hợp lệ: 10.000 ≤ số tiền ≤ 5.000.000 và là bội số của 1.000.
- Là VIP khi `VipExpiresAt > now`.
- Mua gói: số dư < giá → lỗi "Số dư không đủ". Ngược lại trừ tiền; hạn mới = (còn VIP ? VipExpiresAt : now) + số ngày.
- Tình trạng tập (`now` so với `PublishedAt`):
  - `Released`: PublishedAt null hoặc ≤ now → ai cũng xem.
  - `EarlyAccess`: now < PublishedAt ≤ now + 24h → chỉ VIP và Admin.
  - `Upcoming`: PublishedAt > now + 24h → chỉ Admin.
- Khách chưa đăng nhập coi như người thường.
- Bị chặn ở trang Watch: hiện trang thông báo khi nào mở, kèm nút mua VIP (nếu là EarlyAccess). Không ghi lịch sử xem.
- Danh sách tập (trang chi tiết phim và trang xem): tập EarlyAccess có nhãn "VIP", tập Upcoming hiện mờ "Sắp chiếu".

**Giao diện**
- `VipController`: `Index` (số dư, trạng thái VIP, các gói, lịch sử giao dịch), `TopUp` POST, `Buy` POST. Yêu cầu đăng nhập.
- Menu tài khoản thêm mục "Gói VIP"; trang hồ sơ hiện số dư và hạn VIP.

## Để lại cho người làm
- Migration phải sinh bằng `Add-Migration AddVip` trong Visual Studio (file .resx snapshot model chỉ VS tạo được).
