using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebXemPhim.Services;

namespace WebXemPhim.Tests
{
    [TestClass]
    public class AccountRulesTests
    {
        // SCRUM-12, SCRUM-14: từng trường có thông báo riêng
        [TestMethod]
        public void Registration_HopLe_KhongCoLoi()
        {
            Assert.AreEqual(0, AccountRules.ValidateRegistration("qa_new01", "qa@mail.com", "Qa@12345").Count);
        }

        [TestMethod]
        public void Registration_UsernameRong_BaoYeuCauNhap()
        {
            var e = AccountRules.ValidateRegistration("", "qa@mail.com", "Qa@12345");
            StringAssert.Contains(e["Username"], "nhập Tên đăng nhập");
        }

        [TestMethod]
        public void Registration_MatKhauRong_BaoBatBuoc()
        {
            var e = AccountRules.ValidateRegistration("qa01", "qa@mail.com", "");
            StringAssert.Contains(e["Password"], "nhập Mật khẩu");
        }

        [DataTestMethod]
        [DataRow("1")]
        [DataRow("12345")]
        public void Registration_MatKhauNganHon6_BiTuChoi(string pw)
        {
            var e = AccountRules.ValidateRegistration("qa01", "qa@mail.com", pw);
            StringAssert.Contains(e["Password"], "tối thiểu 6");
        }

        [TestMethod]
        public void Registration_MatKhau6KyTu_HopLe()
        {
            Assert.IsNull(AccountRules.ValidatePassword("123456"));
        }

        [TestMethod]
        public void Registration_EmailRong_BaoBatBuoc()
        {
            var e = AccountRules.ValidateRegistration("qa01", "", "Qa@12345");
            StringAssert.Contains(e["Email"], "nhập Email");
        }

        [DataTestMethod]
        [DataRow("abc")]
        [DataRow("abc@")]
        [DataRow("a b@mail.com")]
        public void Registration_EmailSaiDinhDang_BiTuChoi(string email)
        {
            StringAssert.Contains(AccountRules.ValidateEmail(email), "định dạng");
        }

        // TC_AUTH_008: biên 100 / 101 ký tự
        [TestMethod]
        public void Username_100KyTu_HopLe()
        {
            Assert.IsNull(AccountRules.ValidateUsername(new string('a', 100)));
        }

        [TestMethod]
        public void Username_101KyTu_BiTuChoi()
        {
            StringAssert.Contains(AccountRules.ValidateUsername(new string('a', 101)), "tối đa 100");
        }

        // SCRUM-13 / TC_AUTH_017: thẻ script bị từ chối bằng thông báo thân thiện
        [DataTestMethod]
        [DataRow("<script>alert(1)</script>")]
        [DataRow("ten co dau cach")]
        [DataRow("tênđăngnhập")]
        public void Username_KyTuKhongHopLe_BiTuChoi(string username)
        {
            Assert.IsNotNull(AccountRules.ValidateUsername(username));
        }

        // SCRUM-19: cắt khoảng trắng
        [TestMethod]
        public void Normalize_CatKhoangTrangDauCuoi()
        {
            Assert.AreEqual("qa_trim01", AccountRules.Normalize("   qa_trim01  "));
            Assert.IsNull(AccountRules.ValidateUsername("   qa_trim01  "));
        }

        // SCRUM-11, SCRUM-22
        [DataTestMethod]
        [DataRow("a.jpg")]
        [DataRow("a.JPEG")]
        [DataRow("anh nen.png")]
        [DataRow("x.webp")]
        [DataRow("x.gif")]
        public void IsAllowedImage_FileAnh_True(string name)
        {
            Assert.IsTrue(AccountRules.IsAllowedImage(name));
        }

        [DataTestMethod]
        [DataRow("virus.exe")]
        [DataRow("a.jpg.exe")]
        [DataRow("script.aspx")]
        [DataRow("noext")]
        [DataRow("")]
        [DataRow(null)]
        public void IsAllowedImage_KhongPhaiAnh_False(string name)
        {
            Assert.IsFalse(AccountRules.IsAllowedImage(name));
        }

        // SCRUM-18
        [TestMethod]
        public void DisplayName_KhongCoBietDanh_DungUsername()
        {
            Assert.AreEqual("qa_new01", AccountRules.DisplayName(null, "qa_new01"));
            Assert.AreEqual("qa_new01", AccountRules.DisplayName("  ", "qa_new01"));
            Assert.AreEqual("Bình", AccountRules.DisplayName("Bình", "qa_new01"));
        }
    }
}
