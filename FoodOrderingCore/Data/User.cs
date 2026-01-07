using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class User
    {
        public long Id { set; get; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { set; get; }
        [Column(TypeName = "nvarchar(256)")]
        public string Email { set; get; }
        [Column(TypeName = "varchar(15)")]
        public string Phone { set; get; }
        public string Password { set; get; }
        [Column(TypeName = "money")]
        public decimal WalletAmount { set; get; }
        [Column(TypeName = "nvarchar(max)")]
        public string TempCartMeta { set; get; }
        public int RoleId { set; get; }
        public Role Role { set; get; }
        public ICollection<Order> Orders { set; get; }
    }
}
