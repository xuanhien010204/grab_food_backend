using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingCore.Data
{
    public class Store
    {
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string Address { set; get; }

        [Column(TypeName = "varchar(20)")]
        public string Latitude { set; get; }

        [Column(TypeName = "varchar(20)")]
        public string Longitude { set; get; }

        [Column(TypeName = "varchar(max)")]
        public string ImageSrc { set; get; }

        [Column(TypeName = "varchar(15)")]
        public string Phone { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string OpenTime { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string CloseTime { get; set; }
        // Is store currently open?
        public bool IsOpen { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Average rating (1-5)
        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0;

        // Total number of reviews
        public int ReviewCount { get; set; } = 0;

        // Minimum order amount
        [Column(TypeName = "money")]
        public decimal MinOrderAmount { get; set; } = 0;

        // Delivery fee
        [Column(TypeName = "money")]
        public decimal DeliveryFee { get; set; } = 0;

        // Estimated delivery time in minutes
        public int EstimatedDeliveryTime { get; set; } = 30;

        public int TenantId { get; set; }

        public Tenant Tenant { get; set; }
        public ICollection<FoodStore> FoodStores { set; get; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Voucher> Vouchers { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
