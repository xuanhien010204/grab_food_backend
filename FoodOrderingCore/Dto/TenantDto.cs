using System;
using System.Collections.Generic;
using System.Text;

namespace FoodOrderingCore.Dto
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
