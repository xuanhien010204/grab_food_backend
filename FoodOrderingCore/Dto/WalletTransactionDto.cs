using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Dto
{
    // Dto for wallet transaction history (Deposit only)
    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionTypeName => TransactionType.ToString();
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public TransactionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string Description { get; set; }
        public string ExternalReference { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
