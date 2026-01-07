using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrderingCore.Request
{
    public class FoodStoreFilterRequest : ParentFilterRequest
    {
        public string FoodName { set; get; }
        public int? FoodTypeId { set; get; }
    }
}
