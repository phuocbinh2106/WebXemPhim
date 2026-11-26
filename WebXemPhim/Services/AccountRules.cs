using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebXemPhim.Services
{
    /// <summary>
    /// Luật kiểm tra dữ liệu tài khoản (M1). Không phụ thuộc System.Web để unit test được.
    /// </summary>
    public static class AccountRules
    {
        public const int UsernameMaxLength = 100;
        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 100;
        public const int EmailMaxLength = 255;

        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static readonly Regex UsernamePattern = new Regex(@"^[A-Za-z0-9_.\-]+$", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new Regex(@"^[^@\s<>]+@[^@\s<>]+\.[^@\s<>]+$", RegexOptions.Compiled);

        /// <summary>Cắt khoảng trắng đầu/cuối (SCRUM-19).</summary>
        public static string Normalize(string value)
        {
            return value == null ? null : value.Trim();
        }

        public static string ValidateUsername(string username)
        {
            username = Normalize(username);
            if (string.IsNullOrEmpty(username))
                return "Vui lòng nhập Tên đăng nhập.";
            if (username.Length > UsernameMaxLength)
                return "Tên đăng nhập tối đa " + UsernameMaxLength + " ký tự.";
            if (!UsernamePattern.IsMatch(username))
                return "Tên đăng nhập chỉ gồm chữ không dấu, số và các ký tự _ . -";
            return null;
        }

        public static string ValidateEmail(string email)
        {
            email = Normalize(email);
            if (string.IsNullOrEmpty(email))
                return "Vui lòng nhập Email.";
            if (email.Length > EmailMaxLength)
                return "Email tối đa " + EmailMaxLength + " ký tự.";
            if (!EmailPattern.IsMatch(email))
                return "Email không đúng định dạng.";
            return null;
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Vui lòng nhập Mật khẩu.";
            if (password.Length < PasswordMinLength)
                return "Mật khẩu tối thiểu " + PasswordMinLength + " ký tự.";
            if (password.Length > PasswordMaxLength)
                return "Mật khẩu tối đa " + PasswordMaxLength + " ký tự.";
            return null;
        }

        /// <summary>
        /// Kiểm tra form Đăng ký (SCRUM-12, SCRUM-14). Trả về danh sách lỗi theo tên trường, rỗng nếu hợp lệ.
        /// </summary>
        public static Dictionary<string, string> ValidateRegistration(string username, string email, string password)
        {
            var errors = new Dictionary<string, string>();
            Add(errors, "Username", ValidateUsername(username));
            Add(errors, "Email", ValidateEmail(email));
            Add(errors, "Password", ValidatePassword(password));
            return errors;
        }

        /// <summary>Chỉ chấp nhận file ảnh cho avatar/ảnh nền (SCRUM-11, SCRUM-22).</summary>
        public static bool IsAllowedImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string ext;
            try { ext = Path.GetExtension(fileName); }
            catch (ArgumentException) { return false; }
            return !string.IsNullOrEmpty(ext) && AllowedImageExtensions.Contains(ext.ToLowerInvariant());
        }

        public static string AllowedImageExtensionsText
        {
            get { return string.Join(", ", AllowedImageExtensions); }
        }

        /// <summary>Tên hiển thị trên menu: biệt danh, nếu trống thì tên đăng nhập (SCRUM-18).</summary>
        public static string DisplayName(string nickname, string username)
        {
            return string.IsNullOrWhiteSpace(nickname) ? username : nickname.Trim();
        }

        private static void Add(Dictionary<string, string> errors, string key, string message)
        {
            if (message != null) errors[key] = message;
        }
    }
}
