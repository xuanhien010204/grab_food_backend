using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class FoodTypeCreateRequest
    {
        [Required]
        [MaxLength(256)]
        public string Name { set; get; }
        public string ImgSrc { set; get; } = string.Empty;
    }

    public class FoodTypeUpdateRequest : FoodTypeCreateRequest
    {
        [Required]
        public int Id { set; get; }
    }
}