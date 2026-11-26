using System;
using System.ComponentModel.DataAnnotations;

namespace WebXemPhim.Models
{
    /// <summary>Lịch sử giao dịch VIP: nạp tiền (Amount dương) và mua gói (Amount âm).</summary>
    public class VipTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        [Required, StringLength(20)]
        public string Type { get; set; } // "TopUp" hoặc "BuyVip"

        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
