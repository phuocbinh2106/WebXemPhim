using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebXemPhim.Tests
{
    // Lỗi font trang VIP ("GÃ³i VIP"): view .cshtml chứa tiếng Việt lưu UTF-8 không BOM
    // bị Razor/IIS đọc theo bảng mã ANSI (Windows-1252). Test này chặn lỗi tái diễn:
    // mọi file .cshtml có ký tự ngoài ASCII phải được lưu UTF-8 có BOM.
    [TestClass]
    public class FileEncodingTests
    {
        private static string FindViewsFolder()
        {
            var dir = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                var views = Path.Combine(dir.FullName, "WebXemPhim", "Views");
                if (Directory.Exists(views)) return views;
                dir = dir.Parent;
            }
            return null;
        }

        private static bool HasUtf8Bom(byte[] b)
        {
            return b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF;
        }

        [TestMethod]
        public void View_CoTiengViet_PhaiLuuUtf8CoBom()
        {
            var views = FindViewsFolder();
            if (views == null) Assert.Inconclusive("Không tìm thấy thư mục WebXemPhim/Views.");

            var loi = Directory.GetFiles(views, "*.cshtml", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var b = File.ReadAllBytes(f);
                    return b.Any(x => x > 127) && !HasUtf8Bom(b);
                })
                .Select(f => f.Substring(views.Length))
                .ToList();

            Assert.AreEqual(0, loi.Count,
                "Các view có tiếng Việt chưa lưu UTF-8 có BOM: " + string.Join(", ", loi));
        }
    }
}
