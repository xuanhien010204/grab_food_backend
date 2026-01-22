using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FoodOrderingCore.Data
{
    public class Tenant
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public ICollection<Store> Stores { get; set; }
    }
}