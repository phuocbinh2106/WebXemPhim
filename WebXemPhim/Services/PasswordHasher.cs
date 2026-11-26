using System;
using System.Security.Cryptography;

namespace WebXemPhim.Services
{
    /// <summary>
    /// Băm mật khẩu bằng PBKDF2 (SCRUM-7). Định dạng lưu: PBKDF2$số-vòng$salt-base64$hash-base64.
    /// Tài khoản cũ còn lưu mật khẩu thô vẫn đăng nhập được (so sánh phân biệt hoa/thường)
    /// và được nâng cấp sang dạng băm ngay sau lần đăng nhập đúng đầu tiên.
    /// </summary>
    public static class PasswordHasher
    {
        public const string Prefix = "PBKDF2$";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            var hash = Derive(password, salt, Iterations);
            return Prefix + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool IsHashed(string stored)
        {
            return stored != null && stored.StartsWith(Prefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Kiểm tra mật khẩu. needsUpgrade = true khi mật khẩu đúng nhưng đang lưu dạng thô.
        /// </summary>
        public static bool Verify(string password, string stored, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (password == null || string.IsNullOrEmpty(stored)) return false;

            if (!IsHashed(stored))
            {
                // Dữ liệu cũ: so sánh chính xác từng ký tự (phân biệt hoa/thường)
                bool ok = string.Equals(password, stored, StringComparison.Ordinal);
                needsUpgrade = ok;
                return ok;
            }

            var parts = stored.Split('$');
            if (parts.Length != 4) return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0) return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = Derive(password, salt, iterations, expected.Length);
            return FixedTimeEquals(actual, expected);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int length = HashSize)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(length);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
