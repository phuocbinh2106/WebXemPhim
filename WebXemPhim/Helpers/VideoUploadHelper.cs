using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebXemPhim.Helpers
{
    /// <summary>
    /// Lưu file video người dùng chọn từ máy lên thư mục ~/Upload/Videos của server.
    /// </summary>
    public static class VideoUploadHelper
    {
        public const string VirtualFolder = "~/Upload/Videos/";
        public const string UrlFolder = "/Upload/Videos/";

        // Các định dạng trình duyệt phát được bằng thẻ <video>
        public static readonly string[] AllowedExtensions = { ".mp4", ".m4v", ".webm", ".ogg", ".ogv" };

        public static string AllowedExtensionsText
        {
            get { return string.Join(", ", AllowedExtensions); }
        }

        public static bool HasFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName);
        }

        public static bool IsAllowed(HttpPostedFileBase file)
        {
            if (!HasFile(file)) return false;
            var ext = Path.GetExtension(file.FileName);
            return !string.IsNullOrEmpty(ext)
                && AllowedExtensions.Contains(ext.ToLowerInvariant());
        }

        /// <summary>
        /// Lưu file và trả về đường dẫn URL (vd: /Upload/Videos/tap-1_20261126.mp4).
        /// Tên file được đổi sang dạng không dấu và thêm mốc thời gian để không ghi đè file cũ.
        /// </summary>
        public static string Save(HttpPostedFileBase file, HttpServerUtilityBase server)
        {
            if (!IsAllowed(file))
                throw new InvalidOperationException("File video không hợp lệ.");

            var folder = server.MapPath(VirtualFolder);
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var baseName = ToSafeName(Path.GetFileNameWithoutExtension(Path.GetFileName(file.FileName)));
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            var fileName = baseName + "_" + stamp + ext;
            var counter = 1;
            while (File.Exists(Path.Combine(folder, fileName)))
            {
                fileName = baseName + "_" + stamp + "_" + counter + ext;
                counter++;
            }

            file.SaveAs(Path.Combine(folder, fileName));
            return UrlFolder + fileName;
        }

        /// <summary>Bỏ dấu tiếng Việt, chỉ giữ chữ, số, '-' và '_'.</summary>
        public static string ToSafeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "video";

            var normalized = name.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var safe = Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"[^A-Za-z0-9_-]+", "-").Trim('-');
            if (safe.Length > 80) safe = safe.Substring(0, 80).Trim('-');
            return safe.Length == 0 ? "video" : safe.ToLowerInvariant();
        }
    }
}
