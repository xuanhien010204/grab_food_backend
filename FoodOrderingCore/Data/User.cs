using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class User
    {
        public long Id { set; get; }

        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }

        [Column(TypeName = "nvarchar(256)")]
        public string Email { set; get; }

        [Column(TypeName = "varchar(15)")]
        public string Phone { set; get; }

        public string Password { set; get; }

        [Column(TypeName = "money")]
        public decimal WalletAmount { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string TempCartMeta { set; get; }
        [Column(TypeName = "varchar(500)")]
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public int RoleId { set; get; }

        public Role Role { set; get; }
        public ICollection<Order> Orders { set; get; }
        public ICollection<WalletTransaction> WalletTransactions { get; set; }
        public ICollection<DeliveryAddress> DeliveryAddresses { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<VoucherUsage> VoucherUsages { get; set; }
        public ICollection<Store> ManagedStores { get; set; }
        public ICollection<WithdrawalRequest> WithdrawalRequests { get; set; }
        public ICollection<WithdrawalRequest> ProcessedWithdrawals { get; set; }
        public ICollection<ChatMessage> SentMessages { get; set; }
        public ICollection<ChatMessage> ReceivedMessages { get; set; }
    }
}
