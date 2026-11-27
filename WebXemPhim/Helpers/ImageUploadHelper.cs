using System;
using System.IO;
using System.Web;
using WebXemPhim.Services;

namespace WebXemPhim.Helpers
{
    /// <summary>
    /// Lưu file ảnh (avatar, ảnh nền phim). Chỉ nhận đuôi ảnh (SCRUM-11, SCRUM-22) và luôn
    /// đặt tên file mới để không ghi đè ảnh đã có (SCRUM-23).
    /// </summary>
    public static class ImageUploadHelper
    {
        public static bool HasFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName);
        }

        public static bool IsAllowed(HttpPostedFileBase file)
        {
            return HasFile(file) && AccountRules.IsAllowedImage(Path.GetFileName(file.FileName));
        }

        public static string InvalidMessage
        {
            get { return "Chỉ chấp nhận file ảnh: " + AccountRules.AllowedImageExtensionsText; }
        }

        /// <summary>Lưu ảnh vào thư mục ảo (vd "~/Upload/Banner/") và trả về URL (vd "/Upload/Banner/ten_2026...jpg").</summary>
        public static string Save(HttpPostedFileBase file, HttpServerUtilityBase server, string virtualFolder)
        {
            if (!IsAllowed(file))
                throw new InvalidOperationException(InvalidMessage);

            var folder = server.MapPath(virtualFolder);
            Directory.CreateDirectory(folder);

            var original = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(original).ToLowerInvariant();
            var baseName = VideoUploadHelper.ToSafeName(Path.GetFileNameWithoutExtension(original));
            if (baseName == "video") baseName = "anh";
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            var fileName = baseName + "_" + stamp + ext;
            var counter = 1;
            while (File.Exists(Path.Combine(folder, fileName)))
            {
                fileName = baseName + "_" + stamp + "_" + counter + ext;
                counter++;
            }

            file.SaveAs(Path.Combine(folder, fileName));
            return virtualFolder.TrimStart('~').TrimEnd('/') + "/" + fileName;
        }
    }
}
