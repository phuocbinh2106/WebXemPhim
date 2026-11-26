using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebXemPhim.Services;

namespace WebXemPhim.Tests
{
    [TestClass]
    public class ViewRulesTests
    {
        // SCRUM-28 / TC_VIEW_011
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(6)]
        [DataRow(-1)]
        public void Rating_NgoaiKhoang_KhongHopLe(int v)
        {
            Assert.IsFalse(ViewRules.IsValidRating(v));
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(5)]
        public void Rating_1Den5_HopLe(int v)
        {
            Assert.IsTrue(ViewRules.IsValidRating(v));
        }

        [TestMethod]
        public void Average_LamTron2ChuSo_RongTraVe0()
        {
            Assert.AreEqual(0, ViewRules.Average(new int[0]));
            Assert.AreEqual(3.67, ViewRules.Average(new[] { 3, 4, 4 }));
        }

        // SCRUM-30 / TC_VIEW_022
        [DataTestMethod]
        [DataRow(-1, 3, 1)]
        [DataRow(0, 3, 1)]
        [DataRow(2, 3, 2)]
        [DataRow(99, 3, 3)]
        [DataRow(5, 0, 1)]
        public void ClampPage(int page, int total, int expected)
        {
            Assert.AreEqual(expected, ViewRules.ClampPage(page, total));
        }

        [DataTestMethod]
        [DataRow(0, 8, 1)]
        [DataRow(8, 8, 1)]
        [DataRow(9, 8, 2)]
        [DataRow(17, 8, 3)]
        public void TotalPages(int items, int size, int expected)
        {
            Assert.AreEqual(expected, ViewRules.TotalPages(items, size));
        }

        // SCRUM-34 / TC_VIEW_019
        [TestMethod]
        public void Search_KhongDau_TimRaPhimCoDau()
        {
            Assert.IsTrue(ViewRules.MatchesSearch("tien nghich", "Tiên Nghịch", null));
            Assert.IsTrue(ViewRules.MatchesSearch("DAU PHA", "Đấu Phá Thương Khung"));
            Assert.IsTrue(ViewRules.MatchesSearch("Tiên  Nghịch", "Tiên Nghịch Movie"));
            Assert.IsTrue(ViewRules.MatchesSearch("renegade", "Tiên Nghịch", "Renegade Immortal"));
        }

        [TestMethod]
        public void Search_KhongKhop_HoacTuKhoaRong_False()
        {
            Assert.IsFalse(ViewRules.MatchesSearch("na tra", "Tiên Nghịch"));
            Assert.IsFalse(ViewRules.MatchesSearch("", "Tiên Nghịch"));
            Assert.IsFalse(ViewRules.MatchesSearch(null, "Tiên Nghịch"));
        }

        [TestMethod]
        public void NormalizeForSearch_BoDauVaChuThuong()
        {
            Assert.AreEqual("dau la dai luc", ViewRules.NormalizeForSearch("  Đấu La   Đại Lục "));
        }
    }
}
