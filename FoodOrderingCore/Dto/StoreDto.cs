namespace FoodOrderingCore.Dto
{
    public class StoreDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
        public string ImageSrc { set; get; }
        public IEnumerable<FoodStoreDto> FoodStores { set; get; }
    }
}
