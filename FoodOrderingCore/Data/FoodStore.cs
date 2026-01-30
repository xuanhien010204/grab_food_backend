using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class FoodStore
    {
        public Guid Id { get; set; }
        public long StoreId { set; get; }
        public Store Store { set; get; }
        public long FoodId { set; get; }
        public Food Food { set; get; }
        public int? SizeId { get; set; }
        public FoodSize Size { get; set; }
        [Column(TypeName = "money")]
        public decimal Price { set; get; }
        public bool IsAvailable { get; set; } = true;
        public ICollection<OrderDetail> OrderDetails { set; get; }
    }
}
