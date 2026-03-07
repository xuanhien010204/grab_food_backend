using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class CreateWithdrawalRequest
    {
        [Required]
        [Range(10000, double.MaxValue, ErrorMessage = "Số tiền rút tối thiểu là 10,000đ")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankAccount { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }
    }

    public class ProcessWithdrawalRequest
    {
        [MaxLength(500)]
        public string AdminNote { get; set; }
    }
}
