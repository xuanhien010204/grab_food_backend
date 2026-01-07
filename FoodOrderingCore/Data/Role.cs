using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Role
    {
        public int Id { set; get; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }
        public ICollection<User> Users { get; set; }
    }
}
