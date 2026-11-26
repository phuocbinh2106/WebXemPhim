using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace WebXemPhim.Services
{
    /// <summary>
    /// Luật cho phần xem phim và tương tác (M3): phân trang, đánh giá, tìm kiếm không dấu.
    /// </summary>
    public static class ViewRules
    {
        public const int MinRating = 1;
        public const int MaxRating = 5;

        public static bool IsValidRating(int value)
        {
            return value >= MinRating && value <= MaxRating;
        }

        public static int TotalPages(int totalItems, int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (totalItems <= 0) return 1;
            return (int)Math.Ceiling((double)totalItems / pageSize);
        }

        /// <summary>Đưa số trang về khoảng 1 .. totalPages (SCRUM-30).</summary>
        public static int ClampPage(int page, int totalPages)
        {
            if (totalPages < 1) totalPages = 1;
            if (page < 1) return 1;
            if (page > totalPages) return totalPages;
            return page;
        }

        /// <summary>Điểm trung bình làm tròn 2 chữ số, 0 nếu chưa có đánh giá.</summary>
        public static double Average(IEnumerable<int> values)
        {
            var list = (values ?? Enumerable.Empty<int>()).ToList();
            return list.Count == 0 ? 0 : Math.Round(list.Average(), 2);
        }

        /// <summary>Bỏ dấu tiếng Việt, chữ thường, gộp khoảng trắng (SCRUM-34).</summary>
        public static string NormalizeForSearch(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var decomposed = text.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);
            bool lastSpace = false;
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsWhiteSpace(c))
                {
                    if (!lastSpace && sb.Length > 0) sb.Append(' ');
                    lastSpace = true;
                    continue;
                }
                sb.Append(char.ToLowerInvariant(c));
                lastSpace = false;
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).Trim();
        }

        /// <summary>Phim khớp từ khóa nếu Tên hoặc Tên khác chứa từ khóa (không phân biệt dấu, hoa/thường).</summary>
        public static bool MatchesSearch(string query, params string[] fields)
        {
            var q = NormalizeForSearch(query);
            if (q.Length == 0) return false;
            return fields != null && fields.Any(f => NormalizeForSearch(f).Contains(q));
        }
    }
}
