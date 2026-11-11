# Hướng dẫn thuyết trình: Test Scenario và Test Case của WebXemPhim

File đi kèm: `Test_Scenario_WebXemPhim_30TC.xlsx`. File có 3 sheet:

| Sheet | Nội dung |
|---|---|
| Test Scenario | 23 kịch bản kiểm thử, mỗi kịch bản gắn với một tiêu chí nghiệm thu (AC) trong slide |
| Test Case | 30 test case chi tiết (9 AUTH, 10 FILM, 11 VIEW) |
| Đối chiếu Web | Với từng test case: chức năng có trong web không, bằng chứng trong code, kết quả dự đoán |

> Lưu ý: cột "Dự đoán kết quả" được suy ra từ việc đọc mã nguồn, **chưa phải kết quả chạy thật**.
> Trước khi thuyết trình, hãy chạy web trên localhost, làm theo từng test case, rồi ghi
> Actual Result, Status (Pass/Fail) và Defect ID (mã bug Jira, ví dụ WXP-1) vào sheet Test Case.

---

## 1. Thứ tự trình bày (khoảng 8–10 phút)

### Bước 1: Nối với phần trước (30 giây)
> "Ở phần trước nhóm đã chọn 3 module trọng tâm và viết 21 tiêu chí nghiệm thu (AC).
> Phần này nhóm biến các AC đó thành Test Scenario và Test Case để chạy thật trên web."

### Bước 2: Giải thích khác nhau giữa Scenario và Case (1 phút)
Mở sheet **Test Scenario** và nói:
- **Test Scenario** trả lời câu hỏi *"Cần kiểm tra cái gì?"*. Ví dụ: TS_AUTH_004 "Kiểm tra đăng nhập sai mật khẩu".
- **Test Case** trả lời câu hỏi *"Kiểm tra cụ thể như thế nào?"*: có điều kiện trước, từng bước, dữ liệu cụ thể và kết quả mong đợi.
- Một scenario có thể có nhiều test case (cột *Associated Test Cases*).

### Bước 3: Giải thích cách đặt mã và truy vết (1 phút)
- Mã scenario `TS_<MODULE>_<số>`, mã test case `TC_<MODULE>_<số>`.
- Cột *Test Scenario Description* bắt đầu bằng mã AC trong slide, ví dụ `[AUTH-05]`.
  → Đi được một chuỗi: **AC (slide) → Scenario → Test Case → Bug Jira**. Chuỗi này gọi là *traceability* (khả năng truy vết).
- Cột *Scenario ID* trong sheet Test Case là công thức trỏ sang sheet Scenario, nên đổi mã ở một chỗ thì chỗ kia tự cập nhật.

### Bước 4: Giải thích các cột Test Type (1 phút)
| Test Type | Ý nghĩa | Ví dụ |
|---|---|---|
| Positive | Nhập dữ liệu đúng, mong hệ thống chạy đúng (Happy Path) | Đăng ký hợp lệ |
| Negative | Nhập dữ liệu sai, mong hệ thống báo lỗi đúng (Unhappy Path) | Sai mật khẩu |
| Boundary | Thử giá trị ở sát biên giới hạn | Username 100 và 101 ký tự |
| Security | Cố tình vượt quyền hoặc tấn công | Tự gán Role = Admin |

### Bước 5: Đi qua 1 test case mẫu thật kỹ (2 phút)
Chọn **TC_AUTH_005** (đăng ký kèm Role = Admin) vì nó dễ hiểu và cho thấy giá trị của kiểm thử:
1. *Pre-conditions*: chưa đăng nhập, mở DevTools.
2. *Test Steps*: thêm ô ẩn `Role=Admin` vào form, đăng ký, đăng nhập, vào `/Admin`.
3. *Expected Result*: Role vẫn là User, bị đẩy về trang chủ.
4. Nếu chạy thật và vào được `/Admin` → Status = **Fail**, Defect ID = **WXP-1**, rồi nối sang phần demo Jira.

### Bước 6: Tóm tắt kết quả (1 phút)
Mở sheet **Đối chiếu Web**, đọc 4 con số ở đầu sheet (tổng TC, số chức năng có trong web, dự đoán Pass/Fail).
Sau khi chạy thật, thay bằng số liệu thật.

### Bước 7: Nêu các bug nổi bật (2 phút)
Chỉ chọn 3–4 bug nặng nhất, mỗi bug nói 1 câu:
- **TC_FILM_007**: User thường xóa được phim vì action POST xóa phim không kiểm tra quyền (Critical, WXP-2).
- **TC_AUTH_005 / 006**: tự nâng quyền lên Admin qua form đăng ký hoặc form hồ sơ (Critical, WXP-1).
- **TC_FILM_003**: sửa phim mà không chọn ảnh mới thì ảnh nền bị mất (Major).
- **TC_VIEW_011**: bấm "Gửi" đánh giá mà quên chọn sao thì trang lỗi 500 (Major).

---

## 2. Giải thích những chỗ dễ bị hỏi

**Vì sao giảm từ 78 xuống 30 test case?**
Giữ đủ 21 AC (mỗi AC có ít nhất 1 test case) và ưu tiên test case có rủi ro cao. Những test case
cùng mục đích được gộp lại (ví dụ: sai mật khẩu và tài khoản không tồn tại cùng ra một thông báo
nên gộp thành TC_AUTH_004). Cột "TC cũ" ở sheet Đối chiếu Web cho biết mỗi test case mới lấy từ test case cũ nào.

**Boundary value analysis (phân tích giá trị biên) là gì?**
Lỗi thường nằm ở sát giới hạn. Cột Username giới hạn 100 ký tự nên test đúng 100 (phải được chấp nhận)
và 101 (phải bị từ chối). Đánh giá sao hợp lệ 1–5 nên test 1, 5 (hợp lệ) và 0, 6, -1 (không hợp lệ).

**Mass Assignment là gì?** (TC_AUTH_005, 006)
Controller nhận nguyên đối tượng `User` từ form. Nếu người dùng tự thêm ô `Role` vào form,
ASP.NET MVC cũng gán luôn giá trị đó vào `User.Role`. Cách sửa: chỉ cho phép bind các trường cần
thiết (`[Bind(Include = "Username,Email,Password")]`) hoặc dùng ViewModel riêng.

**Vì sao phải test bằng URL hoặc DevTools mà không chỉ bấm trên giao diện?** (TC_AUTH_007, TC_FILM_007)
Giao diện ẩn nút "Quản lý phim" với User thường, nhưng ẩn nút không có nghĩa là chặn quyền.
Kẻ tấn công gửi thẳng request được. Web này kiểm tra quyền trong từng action, nên phải test từng URL.

**`__RequestVerificationToken` là gì?** (TC_FILM_007)
Đây là mã chống giả mạo request (Anti-Forgery / CSRF). Server chỉ nhận form POST có mã này.
Nó chống trang web lạ gửi request thay người dùng, nhưng **không thay cho kiểm tra quyền**:
User đã đăng nhập vẫn lấy được mã hợp lệ từ bất kỳ form nào. Vì vậy TC_FILM_007 chỉ cần sửa
`action` của form "Yêu Thích" thành `/Films/Delete/{id}` là gửi được request xóa phim.

**Session là gì?**
Vùng nhớ phía server lưu thông tin người đang đăng nhập (UserId, Role, Avatar...). Đăng nhập tạo
Session, đăng xuất xóa Session. Mọi kiểm tra quyền của web này đều đọc `Session["Role"]`.

**Lỗi 500 và 404 khác nhau thế nào?**
404: không tìm thấy trang hoặc dữ liệu (hành vi đúng khi mở phim không tồn tại). 500: server gặp lỗi
không xử lý được. Người dùng không bao giờ nên thấy lỗi 500, nên mọi test case dữ liệu sai đều kỳ vọng "không lỗi 500".

**Vì sao xóa tập có bình luận lại lỗi?** (TC_FILM_010)
Trong `AppDbContext`, quan hệ Tập–Bình luận được cấu hình không xóa dây chuyền
(`WillCascadeOnDelete(false)`). Khi xóa tập mà bình luận vẫn trỏ tới tập đó, SQL Server chặn lại
bằng lỗi khóa ngoại.

**Severity và Priority khác nhau thế nào?**
Severity là mức độ ảnh hưởng của bug (Critical, Major, Minor). Priority là mức độ cần sửa sớm.
Trong cột "Dự đoán kết quả", chữ trong ngoặc là Severity.

**Status Blocked dùng khi nào?**
Khi chưa thể chạy test case vì thiếu điều kiện (ví dụ chưa có quyền vào SQL Server để kiểm tra
TC_AUTH_009). Blocked khác Fail: Fail là đã chạy và sai, Blocked là chưa chạy được.

---

## 3. Câu hỏi giảng viên có thể hỏi và gợi ý trả lời

| Câu hỏi | Gợi ý trả lời |
|---|---|
| Làm sao biết 30 test case là đủ? | Mỗi AC trong 21 AC có ít nhất 1 test case, nhìn cột Associated Test Cases là thấy. Các test case bỏ đi trùng mục đích hoặc rủi ro thấp (giao diện, responsive). |
| Kết quả mong đợi lấy từ đâu? | Từ AC trong slide và câu hỏi phản biện. Những chỗ yêu cầu chưa rõ (ví dụ Khách có được xem phim không) thì ghi chú cần BA/PO xác nhận. |
| Test case Fail thì làm gì tiếp? | Log bug lên Jira (Summary, Steps, Expected, Actual, Severity), ghi mã bug vào cột Defect ID, dev sửa xong thì retest, đạt mới đóng bug. |
| Tại sao không test hiệu năng, giao diện responsive? | Đã nêu ở phần Out-of-scope: vượt phạm vi môn học và ít rủi ro hơn 3 module trọng tâm. |

---

## 4. Việc cần làm trước buổi thuyết trình

1. Tạo sẵn dữ liệu test: tài khoản `qa_user01`, `qa_user02` (mật khẩu `Qa@12345`), ít nhất 9 phim có tên chứa "Test QA".
2. Chạy theo thứ tự: AUTH → TC_FILM_001 đến TC_FILM_009 → toàn bộ VIEW → cuối cùng TC_FILM_010. Lý do: một số test case dùng dữ liệu của test case trước (TC_FILM_010 cần bình luận tạo ở TC_VIEW_007, và nó xóa Tập 1 mà các test case VIEW đang dùng).
3. Điền Actual Result, Status, Defect ID. Chụp ảnh màn hình các test case Fail để làm attachment cho bug Jira.
4. Điền tên và MSSV ở phần đầu 2 sheet.
