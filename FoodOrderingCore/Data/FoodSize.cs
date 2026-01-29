namespace FoodOrderingCore.Data
{
    public class FoodSize
    {
        public int Id { get; set; }
        public string Name { get; set; }          
        public string Description { get; set; }    
        public int SortOrder { get; set; } 
        
        public ICollection<FoodStore> FoodStores { get; set; }
    }
}
