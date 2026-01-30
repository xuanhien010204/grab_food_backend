using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Data
{
    public class WalletTransaction
    {
        [Key]
        public Guid Id { get; set; }

        public long UserId { get; set; }

        public TransactionType TransactionType { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "money")]
        public decimal BalanceAfter { get; set; }

        public TransactionStatus Status { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ExternalReference { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public User User { get; set; }
    }
}
