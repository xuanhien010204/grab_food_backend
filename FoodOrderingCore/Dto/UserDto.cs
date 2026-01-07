using FoodOrderingCore.Request;

namespace FoodOrderingCore.Dto
{
    public class UserDto
    {
        public long Id { set; get; }
        public string Name { set; get; }
        public string Email { set; get; }
        public string Phone { set; get; }
        public decimal WalletAmount { set; get; }
        public int RoleId { set; get; }
        public string RoleName { set; get; }
        public string TempCartMeta { set; get; }
    }
}
