using System;
using System.Collections.Generic;
using System.Linq;

namespace WebXemPhim.Services
{
    /// <summary>
    /// Luật kiểm tra dữ liệu phim, tập phim, thể loại (M2).
    /// </summary>
    public static class FilmRules
    {
        public const int MinReleaseYear = 1900;
        public const int MaxEpisodeCount = 5000;
        public const int EpisodeTitleMaxLength = 255;
        public const int GenreNameMaxLength = 50;

        /// <summary>Năm phát hành: để trống hoặc trong khoảng 1900 .. năm hiện tại + 2 (SCRUM-24).</summary>
        public static string ValidateReleaseYear(int? year, int currentYear)
        {
            if (!year.HasValue) return null;
            int max = currentYear + 2;
            if (year.Value < MinReleaseYear || year.Value > max)
                return "Năm phát hành không hợp lệ (từ " + MinReleaseYear + " đến " + max + ").";
            return null;
        }

        /// <summary>Số tập: để trống hoặc 0 .. 5000, không nhận số âm (SCRUM-24).</summary>
        public static string ValidateEpisodeCount(int? count)
        {
            if (!count.HasValue) return null;
            if (count.Value < 0)
                return "Số tập không được âm.";
            if (count.Value > MaxEpisodeCount)
                return "Số tập tối đa " + MaxEpisodeCount + ".";
            return null;
        }

        /// <summary>Số thứ tự tập phải lớn hơn 0 (SCRUM-24).</summary>
        public static string ValidateEpisodeNumber(int number)
        {
            if (number <= 0)
                return "Số thứ tự tập phải lớn hơn 0.";
            if (number > MaxEpisodeCount)
                return "Số thứ tự tập tối đa " + MaxEpisodeCount + ".";
            return null;
        }

        /// <summary>Tên tập bắt buộc, tối đa 255 ký tự (SCRUM-26).</summary>
        public static string ValidateEpisodeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Vui lòng nhập Tên tập.";
            if (title.Trim().Length > EpisodeTitleMaxLength)
                return "Tên tập tối đa " + EpisodeTitleMaxLength + " ký tự.";
            return null;
        }

        /// <summary>Tên thể loại bắt buộc, tối đa 50 ký tự (SCRUM-26).</summary>
        public static string ValidateGenreName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Vui lòng nhập Tên thể loại.";
            if (name.Trim().Length > GenreNameMaxLength)
                return "Tên thể loại tối đa " + GenreNameMaxLength + " ký tự.";
            return null;
        }

        /// <summary>So sánh tên không phân biệt hoa/thường và khoảng trắng đầu/cuối (SCRUM-25).</summary>
        public static bool SameName(string a, string b)
        {
            if (a == null || b == null) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Các số thứ tự tập còn thiếu từ 1 đến targetCount, dùng khi tăng Số tập (SCRUM-27).
        /// </summary>
        public static List<int> MissingEpisodeNumbers(IEnumerable<int> existingNumbers, int? targetCount)
        {
            var result = new List<int>();
            if (!targetCount.HasValue || targetCount.Value <= 0) return result;

            var existing = new HashSet<int>(existingNumbers ?? Enumerable.Empty<int>());
            for (int i = 1; i <= Math.Min(targetCount.Value, MaxEpisodeCount); i++)
            {
                if (!existing.Contains(i)) result.Add(i);
            }
            return result;
        }
    }
}
