using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(256)]
        public string Name { set; get; }
        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { set; get; }
        [Required]
        [MaxLength(15)]
        public string Phone { set; get; }
        [Required]
        public string Password { set; get; }
    }
}
