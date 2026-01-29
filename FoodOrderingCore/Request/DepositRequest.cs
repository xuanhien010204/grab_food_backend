using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    // Request to deposit money into wallet via MoMo
    public class DepositRequest
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(10000, 50000000, ErrorMessage = "Amount must be between 10,000 and 50,000,000 VND")]
        public long Amount { get; set; }

        // Optional note for the deposit
        [MaxLength(200)]
        public string Note { get; set; }
    }
}
