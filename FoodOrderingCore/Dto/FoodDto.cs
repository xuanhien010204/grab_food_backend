namespace FoodOrderingCore.Dto
{
    public class FoodDto
    {
        public long Id { set; get; }
        public string Name { set; get; }
        public int FoodTypeId { set; get; }
        public string FoodTypeName { set; get; }
        public string ImageSrc { set; get; }
        public bool IsAvailable { set; get; }
    }
}
