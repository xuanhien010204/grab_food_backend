using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class CreateDeliveryAddressRequest
    {
        [MaxLength(50)]
        public string Label { get; set; } = "Home";

        [Required(ErrorMessage = "RecipientName is required")]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(200)]
        public string AddressDetail { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateDeliveryAddressRequest : CreateDeliveryAddressRequest
    {
    }
}
