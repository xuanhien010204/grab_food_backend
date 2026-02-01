using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Food
    {
        public long Id { set; get; }

        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }

        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        public string ImageSrc { set; get; }

        public int FoodTypeId { set; get; }

        public bool IsAvailable { set; get; }

        public bool HasSize { get; set; } = false;

        // Average rating (1-5)
        public decimal Rating { get; set; } = 0;

        // Total number of reviews
        public int ReviewCount { get; set; } = 0;

        public FoodType FoodType { set; get; }
        public ICollection<FoodStore> FoodStores { set; get; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
