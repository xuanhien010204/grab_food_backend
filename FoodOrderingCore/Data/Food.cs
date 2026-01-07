using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Food
    {
        public long Id { set; get; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }
        public string ImageSrc { set; get; }
        public int FoodTypeId { set; get; }
        public bool IsAvailable { set; get; }
        public ICollection<FoodStore> FoodStores { set; get; }
        public FoodType FoodType { set; get; }
    }
}
