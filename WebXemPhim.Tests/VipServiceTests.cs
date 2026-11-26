using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebXemPhim.Services;

namespace WebXemPhim.Tests
{
    [TestClass]
    public class VipServiceTests
    {
        private static readonly DateTime Now = new DateTime(2026, 11, 26, 10, 0, 0);

        // ===== Nạp tiền =====
        [DataTestMethod]
        [DataRow(10000)]
        [DataRow(11000)]
        [DataRow(5000000)]
        [DataRow(4999000)]
        public void ValidateTopUp_SoTienHopLe_TraVeNull(double amount)
        {
            Assert.IsNull(VipService.ValidateTopUp((decimal)amount));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-10000)]
        [DataRow(9000)]
        [DataRow(9999)]
        [DataRow(5001000)]
        [DataRow(10500)]
        [DataRow(10000.5)]
        public void ValidateTopUp_SoTienKhongHopLe_TraVeLoi(double amount)
        {
            Assert.IsNotNull(VipService.ValidateTopUp((decimal)amount));
        }

        // ===== Trạng thái VIP =====
        [TestMethod]
        public void IsVip_ChuaTungMua_False()
        {
            Assert.IsFalse(VipService.IsVip(null, Now));
        }

        [TestMethod]
        public void IsVip_ConHan_True()
        {
            Assert.IsTrue(VipService.IsVip(Now.AddSeconds(1), Now));
        }

        [TestMethod]
        public void IsVip_HetHanDungLucNay_False()
        {
            Assert.IsFalse(VipService.IsVip(Now, Now));
        }

        [TestMethod]
        public void IsVip_DaHetHan_False()
        {
            Assert.IsFalse(VipService.IsVip(Now.AddDays(-1), Now));
        }

        // ===== Tình trạng tập phim =====
        [TestMethod]
        public void GetAvailability_KhongCoNgayPhatHanh_Released()
        {
            Assert.AreEqual(EpisodeAvailability.Released, VipService.GetAvailability(null, Now));
        }

        [TestMethod]
        public void GetAvailability_PhatHanhDungLucNay_Released()
        {
            Assert.AreEqual(EpisodeAvailability.Released, VipService.GetAvailability(Now, Now));
        }

        [TestMethod]
        public void GetAvailability_DaPhatHanh_Released()
        {
            Assert.AreEqual(EpisodeAvailability.Released, VipService.GetAvailability(Now.AddDays(-3), Now));
        }

        [TestMethod]
        public void GetAvailability_ConMotTick_EarlyAccess()
        {
            Assert.AreEqual(EpisodeAvailability.EarlyAccess, VipService.GetAvailability(Now.AddTicks(1), Now));
        }

        [TestMethod]
        public void GetAvailability_DungMoc24h_EarlyAccess()
        {
            Assert.AreEqual(EpisodeAvailability.EarlyAccess, VipService.GetAvailability(Now.AddHours(24), Now));
        }

        [TestMethod]
        public void GetAvailability_QuaMoc24h_Upcoming()
        {
            Assert.AreEqual(EpisodeAvailability.Upcoming, VipService.GetAvailability(Now.AddHours(24).AddTicks(1), Now));
        }

        // ===== Quyền xem =====
        [DataTestMethod]
        [DataRow(EpisodeAvailability.Released, false, false, true)]
        [DataRow(EpisodeAvailability.Released, true, false, true)]
        [DataRow(EpisodeAvailability.EarlyAccess, false, false, false)]
        [DataRow(EpisodeAvailability.EarlyAccess, true, false, true)]
        [DataRow(EpisodeAvailability.EarlyAccess, false, true, true)]
        [DataRow(EpisodeAvailability.Upcoming, true, false, false)]
        [DataRow(EpisodeAvailability.Upcoming, false, false, false)]
        [DataRow(EpisodeAvailability.Upcoming, false, true, true)]
        public void CanWatch_BangQuyetDinh(EpisodeAvailability availability, bool isVip, bool isAdmin, bool expected)
        {
            Assert.AreEqual(expected, VipService.CanWatch(availability, isVip, isAdmin));
        }

        // ===== Gói VIP =====
        [TestMethod]
        public void Packages_DungGiaVaSoNgay()
        {
            var p1 = VipService.FindPackage("VIP1M");
            var p3 = VipService.FindPackage("VIP3M");
            var p12 = VipService.FindPackage("VIP12M");
            Assert.AreEqual(30, p1.Days); Assert.AreEqual(30000m, p1.Price);
            Assert.AreEqual(90, p3.Days); Assert.AreEqual(79000m, p3.Price);
            Assert.AreEqual(365, p12.Days); Assert.AreEqual(299000m, p12.Price);
        }

        [DataTestMethod]
        [DataRow("VIP99M")]
        [DataRow("")]
        [DataRow((string)null)]
        public void FindPackage_MaKhongTonTai_TraVeNull(string code)
        {
            Assert.IsNull(VipService.FindPackage(code));
        }

        [TestMethod]
        public void FindPackage_KhongPhanBietHoaThuong()
        {
            Assert.IsNotNull(VipService.FindPackage("vip1m"));
        }

        // ===== Mua gói =====
        [TestMethod]
        public void ValidatePurchase_SoDuDungBangGia_HopLe()
        {
            Assert.IsNull(VipService.ValidatePurchase(30000m, VipService.FindPackage("VIP1M")));
        }

        [TestMethod]
        public void ValidatePurchase_ThieuMotDong_Loi()
        {
            Assert.IsNotNull(VipService.ValidatePurchase(29999m, VipService.FindPackage("VIP1M")));
        }

        [TestMethod]
        public void ValidatePurchase_GoiNull_Loi()
        {
            Assert.IsNotNull(VipService.ValidatePurchase(1000000m, null));
        }

        [TestMethod]
        public void CalculateNewExpiry_ChuaTungMua_TinhTuBayGio()
        {
            Assert.AreEqual(Now.AddDays(30), VipService.CalculateNewExpiry(null, 30, Now));
        }

        [TestMethod]
        public void CalculateNewExpiry_ConHan_CongDon()
        {
            var current = Now.AddDays(5);
            Assert.AreEqual(current.AddDays(30), VipService.CalculateNewExpiry(current, 30, Now));
        }

        [TestMethod]
        public void CalculateNewExpiry_DaHetHan_TinhTuBayGio()
        {
            Assert.AreEqual(Now.AddDays(90), VipService.CalculateNewExpiry(Now.AddDays(-10), 90, Now));
        }
    }
}
