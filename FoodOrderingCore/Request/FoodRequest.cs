using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FoodOrderingCore.Request
{
    public class FoodRequest
    {
        [Required]
        public string Name { set; get; }
        public string ImageSrc { set; get; }
        [Required]
        public int FoodTypeId { set; get; }
    }
    public class FoodUpdate : FoodRequest
    {
        public long Id { set; get; }
        public bool IsAvailable { set; get; }
    }
}
