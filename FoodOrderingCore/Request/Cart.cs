using FoodOrderingCore.Dto;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingCore.Request
{
    public class Cart
    {
        [Required]
        public IDictionary<string, CartItem> OrderList { set; get; }
    }
}
