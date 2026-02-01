namespace FoodOrderingCore.Dto
{
    public class DeliveryAddressDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Label { get; set; }
        public string RecipientName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
