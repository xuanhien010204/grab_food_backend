using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class FoodType
    {
        public int Id { set; get; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }
        public string ImgSrc { set; get; }
        public ICollection<Food> Foods { set; get; }
    }
}
