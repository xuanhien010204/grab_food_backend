using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Data
{
    public class WithdrawalRequest
    {
        [Key]
        public Guid Id { get; set; }

        public long ManagerId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string BankAccount { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string BankName { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Note { get; set; }

        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        [Column(TypeName = "nvarchar(500)")]
        public string AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public long? ProcessedByAdminId { get; set; }

        // Navigation properties
        public User Manager { get; set; }
        public User ProcessedByAdmin { get; set; }
    }
}
