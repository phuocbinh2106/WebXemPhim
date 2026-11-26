# Release 2: danh sách bug đã sửa

- **Release 1** = nhánh `release-1` (commit `4af6935`): đủ chức năng chọn file video (#24), VIP (#25), dữ liệu mẫu (#26), chưa sửa bug Jira.
- **Release 2** = nhánh `nopbai` sau khi merge PR sửa lỗi này.

Cách dùng cho báo cáo: chạy bộ test case trên Release 1 trước (ghi cột Release 1), sau đó chạy lại trên Release 2 (ghi cột Release 2) và chuyển trạng thái bug trên Jira sang Done khi TC liên quan Pass.

> Lưu ý: phần upload **video** của SCRUM-22 (TC_FILM_033) đã được sửa sẵn trong PR #24 (chọn file video từ máy, chỉ nhận mp4/m4v/webm/ogg/ogv), nên TC_FILM_033 có thể đã Pass ngay ở Release 1. Phần ảnh nền của SCRUM-22 sửa ở Release 2.

## M1 Xác thực & Phân quyền

| Key | Bug | Cách sửa ở Release 2 | File chính | Test case |
|---|---|---|---|---|
| SCRUM-5 | Đăng ký/cập nhật hồ sơ kèm Role=Admin | Register chỉ nhận Username, Email, Password và luôn gán Role = User; ProfileUpdate bỏ hẳn nhánh sửa Role và CreatedAt | `AccountController` | TC_AUTH_005, 006 |
| SCRUM-6 | Khách/User gửi POST xóa phim được | `Films.DeleteConfirmed` kiểm tra quyền Admin; thiếu token chống CSRF thì chuyển về trang chủ thay vì lỗi | `FilmsController`, `Global.asax.cs` | TC_AUTH_044, TC_FILM_007 |
| SCRUM-7 | Mật khẩu lưu thô, so sánh không phân biệt hoa/thường | Băm PBKDF2-SHA256 (salt ngẫu nhiên, 10.000 vòng). So sánh trong C# nên phân biệt hoa/thường. Tài khoản cũ còn mật khẩu thô vẫn đăng nhập được và tự chuyển sang dạng băm ở lần đăng nhập đầu | `Services/PasswordHasher.cs`, `AccountController` | TC_AUTH_009, 023 |
| SCRUM-8 | Đăng xuất không xóa cookie | `FormsAuthentication.SignOut()`, hủy Session, ghi đè cookie .ASPXAUTH hết hạn; bật `<authentication mode="Forms">` | `Helpers/AuthHelper.cs`, `Web.config` | TC_AUTH_031 |
| SCRUM-9 | User thường xem được trang quản lý thể loại | `Genres/Index` chỉ cho Admin, người khác về trang chủ | `GenresController` | TC_AUTH_045 |
| SCRUM-10 | Đổi Tên đăng nhập trùng vẫn lưu | Kiểm tra trùng Username (và Email) với tài khoản khác trước khi lưu, báo lỗi | `AccountController.ProfileUpdate` | TC_AUTH_050 |
| SCRUM-11 | Upload avatar nhận .exe | Chỉ nhận .jpg .jpeg .png .gif .webp, báo lỗi và không lưu file khác | `Helpers/ImageUploadHelper.cs` | TC_AUTH_049 |
| SCRUM-12 | Đăng ký cho mật khẩu rỗng/1 ký tự, Email rỗng | Bắt buộc Username, Email (đúng định dạng), Mật khẩu tối thiểu 6 ký tự | `Services/AccountRules.cs` | TC_AUTH_012, 014, 019 |
| SCRUM-13 | Nhập "<" gây trang lỗi 500 | Các ô nhập tự do (tên phim, thể loại, tập, bình luận, từ khóa tìm kiếm) cho phép "<" và hiển thị dạng văn bản (Razor tự mã hóa). Tên đăng nhập chứa ký tự lạ bị từ chối bằng thông báo. Trường hợp còn lại chuyển sang trang thông báo thân thiện `/Home/InvalidInput` | `Global.asax.cs`, các controller, `Views/Home/InvalidInput.cshtml` | TC_AUTH_017, TC_FILM_019, TC_VIEW_021, 048 |
| SCRUM-14 | Mọi lỗi Đăng ký đều báo "đã tồn tại" | Thông báo riêng cho từng trường (thiếu, quá dài, sai định dạng, trùng) | `AccountRules`, `Register.cshtml` | TC_AUTH_008, 010 |
| SCRUM-15 | Ghi nhớ đăng nhập không có tác dụng | Ô "Ghi nhớ đăng nhập" đưa vào đúng form đăng nhập; cookie lâu dài 30 ngày; `Global.asax` nạp lại Session từ cookie khi mở lại trình duyệt | `Login.cshtml`, `Global.asax.cs`, `Web.config` | TC_AUTH_027 |
| SCRUM-16 | Nút "Chỉnh Sửa" cạnh Tên đăng nhập ghi đè Ngày tạo | Nút gửi đúng trường (Username, Email, AboutMe); bỏ nút sửa Ngày tạo | `Profile.cshtml`, `AccountController` | TC_AUTH_046 |
| SCRUM-17 | Khách gọi UpdateAvatar gây 500 | Chưa đăng nhập thì chuyển về trang Đăng nhập, không lưu file | `AccountController.UpdateAvatar` | TC_AUTH_034 |
| SCRUM-18 | Menu không hiện tên khi chưa có biệt danh | Hiện biệt danh, nếu trống thì hiện tên đăng nhập | `AuthHelper`, `_Layout.cshtml` | TC_AUTH_002 |
| SCRUM-19 | Username có khoảng trắng đầu/cuối được lưu nguyên | Cắt khoảng trắng khi đăng ký, đăng nhập, sửa hồ sơ | `AccountRules.Normalize` | TC_AUTH_016 |

## M2 Quản lý phim

| Key | Bug | Cách sửa ở Release 2 | File chính | Test case |
|---|---|---|---|---|
| SCRUM-20 | Sửa phim làm mất ảnh nền, Ngày tạo, Lượt xem, Điểm TB | Chỉ cập nhật các trường có trên form; không chọn ảnh mới thì giữ ảnh cũ | `FilmsController.Edit` | TC_FILM_003, 029 |
| SCRUM-21 | Xóa tập đã có bình luận gây 500 | Xóa bình luận và lịch sử xem của tập trước khi xóa tập | `EpisodesController.DeleteConfirmed` | TC_FILM_010 |
| SCRUM-22 | Upload ảnh nền/video không kiểm tra định dạng | Ảnh nền chỉ nhận file ảnh (Release 2); video đã kiểm tra từ PR #24 | `ImageUploadHelper`, `VideoUploadHelper` | TC_FILM_006, 033 |
| SCRUM-23 | Ảnh nền trùng tên ghi đè ảnh phim khác | Luôn lưu tên mới: tên không dấu + mốc thời gian (+ số đếm nếu trùng) | `ImageUploadHelper.Save` | TC_FILM_020 |
| SCRUM-24 | Số tập, Năm, Số thứ tự tập nhận 0/âm/ngoài khoảng | Số tập 0..5000; Năm 1900..năm hiện tại + 2; Số thứ tự tập > 0 | `Services/FilmRules.cs` | TC_FILM_009, 018, 032 |
| SCRUM-25 | Không kiểm tra trùng Tên phim, Số thứ tự tập, Tên thể loại | Báo trùng (không phân biệt hoa/thường, khoảng trắng) | `FilmRules.SameName`, các controller | TC_FILM_022, 031, 044 |
| SCRUM-26 | Tên tập, Tên thể loại để trống/quá dài | Tên tập bắt buộc, tối đa 255; Tên thể loại bắt buộc, tối đa 50 | `FilmRules` | TC_FILM_034, 043, 047 |
| SCRUM-27 | Sửa Số tập 3 lên 5 không tạo thêm tập | Tự tạo các tập còn thiếu (Tập 4, Tập 5) | `FilmRules.MissingEpisodeNumbers`, `FilmsController.Edit` | TC_FILM_028 |

## M3 Xem phim & Tương tác

| Key | Bug | Cách sửa ở Release 2 | File chính | Test case |
|---|---|---|---|---|
| SCRUM-28 | Đánh giá nhận 0, 6, -1 | Chỉ nhận 1..5, báo lỗi, không lưu; cập nhật lại Điểm TB của phim | `FilmsController.AddRating`, `ViewRules` | TC_VIEW_011 |
| SCRUM-29 | Phân trang mất từ khóa/điều kiện lọc | Link phân trang giữ action và tham số (query, type, genreId...); trang Lọc cũng có phân trang | `FilmsController`, `Films/Index.cshtml` | TC_VIEW_002, 026 |
| SCRUM-30 | Tham số URL không hợp lệ gây 500 | page âm/quá lớn đưa về trang hợp lệ; Watch/abc trả 404; Yêu thích phim không tồn tại trả 404 | `ViewRules.ClampPage`, `EpisodesController.Watch`, `FavouritesController.Add` | TC_VIEW_022, 039, 045 |
| SCRUM-31 | Lượt xem không tăng | Mỗi lần mở tập tăng ViewCount của tập và của phim | `EpisodesController.Watch` | TC_VIEW_038 |
| SCRUM-32 | Không bỏ được phim khỏi Yêu thích | Thêm `Favourites/Remove`, nút "Bỏ yêu thích" ở trang Yêu thích và trang chi tiết phim | `FavouritesController`, view | TC_VIEW_044 |
| SCRUM-33 | Khách bấm Yêu thích về trang chủ | Chuyển tới trang Đăng nhập kèm returnUrl, đăng nhập xong quay lại phim | `FavouritesController.Add` | TC_VIEW_008 |
| SCRUM-34 | Tìm không dấu không ra | So khớp sau khi bỏ dấu tiếng Việt và chữ hoa ("tien nghich" ra "Tiên Nghịch") | `ViewRules.MatchesSearch` | TC_VIEW_019 |

## Cải tiến thêm (không có bug Jira)

- Trang chi tiết phim không còn lỗi khi phim chưa có ảnh nền.
- Form đánh giá chỉ hiện khi đã đăng nhập, nút Gửi chỉ bật sau khi chọn sao.
- Sửa thông tin đăng nhập thì cookie xác thực được cấp lại theo tên mới.
- Xóa phim xóa luôn lịch sử xem của các tập.

## Unit test

Luật nghiệp vụ tách ra `WebXemPhim/Services/` (không phụ thuộc System.Web) và được test trong `WebXemPhim.Tests`:
`PasswordHasherTests`, `AccountRulesTests`, `FilmRulesTests`, `ViewRulesTests` (cùng với `VipServiceTests` có sẵn).

Chạy: `dotnet test WebXemPhim.Tests\WebXemPhim.Tests.csproj` hoặc Test Explorer trong Visual Studio.
