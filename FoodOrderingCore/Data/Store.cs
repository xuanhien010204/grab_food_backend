using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Store
    {
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string Address { set; get; }
        [Column(TypeName = "varchar(20)")]
        public string Latitude { set; get; }
        [Column(TypeName = "varchar(20)")]
        public string Longitude { set; get; }
        [Column(TypeName = "varchar(max)")]
        public string ImageSrc { set; get; }
        public ICollection<FoodStore> FoodStores { set; get; }
    }
}
