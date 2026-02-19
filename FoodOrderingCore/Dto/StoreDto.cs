namespace FoodOrderingCore.Dto
{
    public class StoreDto
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
        public string ImageSrc { set; get; }
        public string Phone { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public bool IsOpen { get; set; }
        public bool IsActive { get; set; }
        public long? ManagerId { get; set; }
        public bool IsApproved { get; set; }
        public IEnumerable<FoodStoreDto> FoodStores { set; get; }
    }
}
