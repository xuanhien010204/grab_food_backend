using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class DeliveryAddress
    {
        public long Id { get; set; }
        
        public long UserId { get; set; }
        
        [Column(TypeName = "nvarchar(50)")]
        public string Label { get; set; }
        
        [Column(TypeName = "nvarchar(100)")]
        public string RecipientName { get; set; }
        
        [Column(TypeName = "varchar(15)")]
        public string Phone { get; set; }
        
        [Column(TypeName = "nvarchar(500)")]
        public string Address { get; set; }
        
        [Column(TypeName = "nvarchar(200)")]
        public string AddressDetail { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string Latitude { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string Longitude { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public User User { get; set; }
    }
}
