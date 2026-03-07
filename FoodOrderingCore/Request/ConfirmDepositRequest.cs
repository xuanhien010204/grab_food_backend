using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class ConfirmDepositRequest
    {
        [Required]
        public string OrderId { get; set; }

        public int ResultCode { get; set; }

        public decimal Amount { get; set; }

        public string Message { get; set; }
    }
}
