using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebXemPhim.Services;

namespace WebXemPhim.Tests
{
    /// <summary>SCRUM-7: mật khẩu lưu dạng băm, so sánh phân biệt hoa/thường.</summary>
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        public void Hash_KhongChuaMatKhauGoc_VaCoTienTo()
        {
            var hash = PasswordHasher.Hash("Qa@12345");
            Assert.IsTrue(hash.StartsWith(PasswordHasher.Prefix));
            Assert.IsFalse(hash.Contains("Qa@12345"));
        }

        [TestMethod]
        public void Hash_CungMatKhau_HaiLanKhacNhau_DoSaltNgauNhien()
        {
            Assert.AreNotEqual(PasswordHasher.Hash("Qa@12345"), PasswordHasher.Hash("Qa@12345"));
        }

        [TestMethod]
        public void Verify_MatKhauDung_TraVeTrue()
        {
            bool upgrade;
            var hash = PasswordHasher.Hash("Qa@12345");
            Assert.IsTrue(PasswordHasher.Verify("Qa@12345", hash, out upgrade));
            Assert.IsFalse(upgrade);
        }

        [DataTestMethod]
        [DataRow("qa@12345")]
        [DataRow("QA@12345")]
        [DataRow("Qa@1234")]
        [DataRow("")]
        public void Verify_SaiHoacKhacHoaThuong_TraVeFalse(string input)
        {
            bool upgrade;
            Assert.IsFalse(PasswordHasher.Verify(input, PasswordHasher.Hash("Qa@12345"), out upgrade));
        }

        [TestMethod]
        public void Verify_MatKhauThoCu_Dung_CanNangCap()
        {
            bool upgrade;
            Assert.IsTrue(PasswordHasher.Verify("123123", "123123", out upgrade));
            Assert.IsTrue(upgrade);
        }

        [TestMethod]
        public void Verify_MatKhauThoCu_KhacHoaThuong_TraVeFalse()
        {
            bool upgrade;
            Assert.IsFalse(PasswordHasher.Verify("abcDEF", "abcdef", out upgrade));
            Assert.IsFalse(upgrade);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("PBKDF2$abc$xx$yy")]
        [DataRow("PBKDF2$10000$!!!$???")]
        public void Verify_ChuoiLuuKhongHopLe_TraVeFalse(string stored)
        {
            bool upgrade;
            Assert.IsFalse(PasswordHasher.Verify("123456", stored, out upgrade));
        }
    }
}
