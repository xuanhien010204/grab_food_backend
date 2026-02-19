using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class RegisterManagerRequest
    {
        [Required]
        [MaxLength(256)]
        public string StoreName { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(256)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Latitude { get; set; }

        [MaxLength(20)]
        public string Longitude { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(10)]
        public string OpenTime { get; set; }

        [MaxLength(10)]
        public string CloseTime { get; set; }

        public string ImageSrc { get; set; }
    }
}
