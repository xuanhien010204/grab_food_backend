using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class OrderDetail
    {
        public Guid OrderId { set; get; }
        public Order Order { set; get; }
        public Guid FoodStoreId { set; get; }
        public FoodStore FoodStore { set; get; }
        [Column(TypeName = "money")]
        public decimal Price { set; get; }
        public int Quantity { set; get; }
        [Column(TypeName = "money")]
        public decimal Total { set; get; }
    }
}
