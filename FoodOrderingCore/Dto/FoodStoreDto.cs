
using FoodOrderingCore.Data;

namespace FoodOrderingCore.Dto
{
    public class FoodStoreDto
    {
        public Guid Id { get; set; }
        public long StoreId { set; get; }
        public StoreDto Store { set; get; }
        public long FoodId { set; get; }
        public FoodDto Food { set; get; }
        public int? SizeId { get; set; }
        public FoodSize Size { set; get; }
        public decimal Price { set; get; }
    }
}
