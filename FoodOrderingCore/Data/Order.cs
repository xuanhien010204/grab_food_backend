using System.ComponentModel.DataAnnotations.Schema;


namespace FoodOrderingCore.Data
{
    public class Order
    {
        public Guid Id { set; get; }
        public long UserId { set; get; }
        public DateTime PurchaseDate { get; set; }
        [Column(TypeName = "money")]
        public decimal Total { set; get; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public User User { set; get; }
    }
}
