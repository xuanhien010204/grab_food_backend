using FoodOrderingCore.Enum;

namespace FoodOrderingCore.Dto
{
    public class WithdrawalRequestDto
    {
        public Guid Id { get; set; }
        public long ManagerId { get; set; }
        public string ManagerName { get; set; }
        public decimal Amount { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string Note { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string ProcessedByAdminName { get; set; }
    }
}
