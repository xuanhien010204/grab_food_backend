namespace FoodOrderingCore.Dto
{
    public class OrderDto
    {
        public Guid Id { set; get; }
        public long UserId { set; get; }
        public DateTime PurchaseDate { get; set; }
        public decimal Total { set; get; }
    }
}
