# Hướng dẫn nạp dữ liệu mẫu (SQL Server / SSMS)

File: `Database/SeedData.sql`

## Cách chạy

1. Mở **SQL Server Management Studio (SSMS)**, kết nối tới server mà web đang dùng
   (chuỗi kết nối trong `Web.config` là `Data Source=(local)`, database `WebXemPhimDB`).
2. **File > Open > File...** chọn `Database/SeedData.sql`.
3. Bấm **Execute** (F5). Cuối cùng sẽ có bảng thống kê số dòng của từng bảng.
4. Chạy web, đăng nhập bằng một trong các tài khoản bên dưới.

Script tự xử lý 2 trường hợp:

- **Chưa có database**: tạo `WebXemPhimDB`, tạo bảng giống migration của project và ghi
  `__MigrationHistory`, nên sau này vẫn dùng `Update-Database` / `Add-Migration` bình thường.
- **Đã có database** (đã chạy web hoặc `Update-Database`): chỉ thêm dữ liệu.

Chạy lại nhiều lần không bị trùng. Muốn xóa sạch dữ liệu cũ rồi nạp lại từ đầu, mở script,
tìm dòng `DECLARE @XoaHetDuLieuCu BIT = 0;` và đổi thành `1`.

## Tài khoản mẫu

| Username     | Mật khẩu | Vai trò | Ghi chú |
|--------------|----------|---------|---------|
| `admin`      | `123123` | Admin   | Quản trị (vào trang Admin, thêm/sửa phim) |
| `nguyenvana` | `123456` | User    | Có avatar, nhiều yêu thích/lịch sử xem |
| `tranthib`   | `123456` | User    | Có avatar |
| `levanc`     | `123456` | User    | Có avatar; VIP đã hết hạn (khi có chức năng VIP) |
| `phamthid`   | `123456` | User    | Avatar có tên file tiếng Việt |
| `hoangvane`  | `123456` | User    | Không có avatar |
| `vipuser`    | `123456` | User    | VIP còn 30 ngày (khi có chức năng VIP) |

## Dữ liệu có sẵn

- **10 thể loại**: Hành động, Tiên hiệp, Huyền huyễn, Võ thuật, Phiêu lưu, Hài hước, Thần thoại,
  Khoa học viễn tưởng, Cổ trang, Hoạt hình 3D.
- **20 phim**: 13 phim bộ có tập, 3 phim lẻ (Tiên Nghịch Movie, Na Tra 1, Na Tra 2),
  4 phim chưa có tập nào (Già Thiên, Thần Ấn Vương Tọa, Tru Tiên, Đấu La Đại Lục 2) để test phim trống.
  Ảnh bìa lấy từ `Upload/Banner`.
- **36 tập**, video lấy từ `Upload/Videos`. Phim nào chỉ có một video thì các tập dùng chung video đó.
  Có 3 tập có ngày phát hành **trong tương lai** (Tiên Nghịch tập 3 và Đấu Phá Thương Khung tập 3: +12 giờ,
  Tiên Nghịch tập 4: +3 ngày) để test chức năng xem sớm VIP.
- Bình luận (theo phim và theo tập), đánh giá 1–5 sao (điểm trung bình được tính lại), yêu thích,
  lịch sử xem, lịch chiếu, nguồn video.

Thời gian (ngày tạo, ngày phát hành...) tính theo lúc chạy script, nên chạy lúc nào cũng thấy
"phim mới" và "tập sắp phát hành" hợp lý.

## Chức năng VIP

Phần 4 của script chỉ chạy khi database đã có cột `Users.Balance`, `Users.VipExpiresAt`
và bảng `VipTransactions` (của chức năng VIP). Nếu chưa có thì tự bỏ qua.

Thứ tự khi đã có code VIP:

1. Chạy `SeedData.sql` (nếu chưa có database).
2. Trong Visual Studio, Package Manager Console: `Add-Migration AddVip` rồi `Update-Database`.
3. Chạy lại `SeedData.sql` để nạp số dư, hạn VIP và lịch sử giao dịch mẫu.

## Lưu ý

- Mật khẩu lưu dạng chữ thường vì code đăng nhập hiện tại so sánh trực tiếp.
- Đường dẫn ảnh/video lưu dạng `/Upload/...` giống cách code upload đang lưu.
