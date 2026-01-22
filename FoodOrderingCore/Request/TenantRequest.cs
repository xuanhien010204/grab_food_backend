using System;
using System.Collections.Generic;
using System.Text;

namespace FoodOrderingCore.Request
{
    public class TenantRequest
    {
        public string Name { get; set; }
    }
    public class TenantUpdateRequest : TenantRequest
    {
        public int Id { get; set; }
    }
}
