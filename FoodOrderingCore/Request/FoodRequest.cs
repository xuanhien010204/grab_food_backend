using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FoodOrderingCore.Request
{
    public class FoodRequest
    {
        [Required]
        public string Name;
        public string ImageSrc { set; get; }
        [Required]
        public int FoodTypeId { set; get; }
    }
    public class FoodUpdate : FoodRequest
    {
        public bool IsAvaiable { set; get; }
    }
}
