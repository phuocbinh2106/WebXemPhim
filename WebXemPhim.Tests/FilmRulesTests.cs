using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WebXemPhim.Services;

namespace WebXemPhim.Tests
{
    [TestClass]
    public class FilmRulesTests
    {
        private const int Year = 2026;

        // SCRUM-24 / TC_FILM_018
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-2025)]
        [DataRow(3000)]
        [DataRow(1899)]
        [DataRow(2029)]
        public void ReleaseYear_NgoaiKhoang_BiTuChoi(int y)
        {
            StringAssert.Contains(FilmRules.ValidateReleaseYear(y, Year), "không hợp lệ");
        }

        [DataTestMethod]
        [DataRow(1900)]
        [DataRow(2026)]
        [DataRow(2028)]
        public void ReleaseYear_TrongKhoang_HopLe(int y)
        {
            Assert.IsNull(FilmRules.ValidateReleaseYear(y, Year));
        }

        [TestMethod]
        public void ReleaseYear_DeTrong_HopLe()
        {
            Assert.IsNull(FilmRules.ValidateReleaseYear(null, Year));
        }

        // TC_FILM_009
        [TestMethod]
        public void EpisodeCount_0_HopLe_Am_BiTuChoi()
        {
            Assert.IsNull(FilmRules.ValidateEpisodeCount(0));
            Assert.IsNull(FilmRules.ValidateEpisodeCount(null));
            Assert.IsNotNull(FilmRules.ValidateEpisodeCount(-1));
            Assert.IsNotNull(FilmRules.ValidateEpisodeCount(5001));
        }

        // TC_FILM_032
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void EpisodeNumber_KhongDuong_BiTuChoi(int n)
        {
            StringAssert.Contains(FilmRules.ValidateEpisodeNumber(n), "lớn hơn 0");
        }

        [TestMethod]
        public void EpisodeNumber_1_HopLe()
        {
            Assert.IsNull(FilmRules.ValidateEpisodeNumber(1));
        }

        // SCRUM-26
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void EpisodeTitle_Rong_BaoBatBuoc(string t)
        {
            StringAssert.Contains(FilmRules.ValidateEpisodeTitle(t), "nhập Tên tập");
        }

        [TestMethod]
        public void EpisodeTitle_Qua255_BiTuChoi()
        {
            Assert.IsNull(FilmRules.ValidateEpisodeTitle(new string('t', 255)));
            Assert.IsNotNull(FilmRules.ValidateEpisodeTitle(new string('t', 256)));
        }

        // TC_FILM_043, TC_FILM_047
        [TestMethod]
        public void GenreName_RongHoacQua50_BiTuChoi()
        {
            Assert.IsNotNull(FilmRules.ValidateGenreName(""));
            Assert.IsNotNull(FilmRules.ValidateGenreName(new string('g', 300)));
            Assert.IsNull(FilmRules.ValidateGenreName(new string('g', 50)));
        }

        // SCRUM-25
        [TestMethod]
        public void SameName_BoQuaHoaThuongVaKhoangTrang()
        {
            Assert.IsTrue(FilmRules.SameName(" Hành Động ", "hành động"));
            Assert.IsFalse(FilmRules.SameName("Hành động", "Hài hước"));
            Assert.IsFalse(FilmRules.SameName(null, "a"));
        }

        // SCRUM-27 / TC_FILM_028
        [TestMethod]
        public void MissingEpisodes_Tang3Len5_TaoTap4Va5()
        {
            CollectionAssert.AreEqual(new[] { 4, 5 }, FilmRules.MissingEpisodeNumbers(new[] { 1, 2, 3 }, 5).ToArray());
        }

        [TestMethod]
        public void MissingEpisodes_CoLoHong_BuDu()
        {
            CollectionAssert.AreEqual(new[] { 2, 4 }, FilmRules.MissingEpisodeNumbers(new[] { 1, 3 }, 4).ToArray());
        }

        [TestMethod]
        public void MissingEpisodes_GiamHoacRong_KhongTaoGi()
        {
            Assert.AreEqual(0, FilmRules.MissingEpisodeNumbers(new[] { 1, 2, 3 }, 2).Count);
            Assert.AreEqual(0, FilmRules.MissingEpisodeNumbers(new int[0], null).Count);
            Assert.AreEqual(0, FilmRules.MissingEpisodeNumbers(new int[0], 0).Count);
        }
    }
}
