using FoodOrderingCore.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingCore.Context
{
    public class FoodOrderingContext : DbContext
    {
        public FoodOrderingContext(DbContextOptions<FoodOrderingContext> options) : base(options) { }

        // Core entities
        public virtual DbSet<User> Users { set; get; }
        public virtual DbSet<Role> Roles { set; get; }
        public virtual DbSet<Store> Stores { set; get; }
        public virtual DbSet<Food> Foods { set; get; }
        public virtual DbSet<FoodSize> FoodSizes { get; set; }
        public virtual DbSet<FoodStore> FoodStores { set; get; }
        public virtual DbSet<FoodType> FoodTypes { set; get; }
        public virtual DbSet<Tenant> Tenants { set; get; }

        // Order entities
        public virtual DbSet<Order> Orders { set; get; }
        public virtual DbSet<OrderDetail> OrderDetails { set; get; }

        // Wallet entities
        public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

        // New feature entities
        public virtual DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }
        public virtual DbSet<Favorite> Favorites { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Tenant - Store relationship (1:N)
            builder.Entity<Store>()
                .HasOne(s => s.Tenant)
                .WithMany(t => t.Stores)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FoodStore>()
                .HasIndex(fs => new { fs.StoreId, fs.FoodId, fs.SizeId })
                .IsUnique(true);

            builder.Entity<FoodStore>()
                .HasOne(fs => fs.Store)
                .WithMany(s => s.FoodStores)
                .HasForeignKey(fs => fs.StoreId);

            builder.Entity<FoodStore>()
                .HasOne(fs => fs.Food)
                .WithMany(f => f.FoodStores)
                .HasForeignKey(fs => fs.FoodId);

            builder.Entity<FoodStore>()
                .HasOne(fs => fs.Size)
                .WithMany(s => s.FoodStores)
                .HasForeignKey(fs => fs.SizeId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrderDetail - MUST define composite key first
            builder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.FoodStoreId });

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.FoodStore)
                .WithMany(f => f.OrderDetails)
                .HasForeignKey(od => od.FoodStoreId);

            // Order - Store relationship
            builder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order indexes for performance
            builder.Entity<Order>()
                .HasIndex(o => o.Status);

            builder.Entity<Order>()
                .HasIndex(o => new { o.UserId, o.PurchaseDate });

            builder.Entity<Order>()
                .HasIndex(o => new { o.StoreId, o.Status });

            // User unique email
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);

            // WalletTransaction relationships
            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.User)
                .WithMany(u => u.WalletTransactions)
                .HasForeignKey(wt => wt.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WalletTransaction>()
                .HasIndex(wt => wt.ExternalReference);

            builder.Entity<WalletTransaction>()
                .HasIndex(wt => new { wt.UserId, wt.CreatedAt });

            // Seed data
            builder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Manager" },
                new Role { Id = 3, Name = "Admin" }
            );

            builder.Entity<FoodType>().HasData(
                new FoodType { Id = 1, Name = "Appetizer" },
                new FoodType { Id = 2, Name = "Main Course" },
                new FoodType { Id = 3, Name = "Dessert" },
                new FoodType { Id = 4, Name = "Beverage" }
            );

            // Seed FoodSize data
            builder.Entity<FoodSize>().HasData(
                new FoodSize { Id = 1, Name = "S", Description = "Nhỏ", SortOrder = 1 },
                new FoodSize { Id = 2, Name = "M", Description = "Vừa", SortOrder = 2 },
                new FoodSize { Id = 3, Name = "L", Description = "Lớn", SortOrder = 3 },
                new FoodSize { Id = 4, Name = "XL", Description = "Siêu lớn", SortOrder = 4 }
            );

            builder.Entity<Tenant>().HasData(
                new Tenant 
                { 
                    Id = 1, 
                    Name = "Default Tenant", 
                    CreateTime = new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Utc),
                    UpdateTime = new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // DeliveryAddress
            builder.Entity<DeliveryAddress>()
                .HasOne(da => da.User)
                .WithMany(u => u.DeliveryAddresses)
                .HasForeignKey(da => da.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DeliveryAddress>()
                .HasIndex(da => new { da.UserId, da.IsDefault });

            // Review
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Store)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Food)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FoodId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasIndex(r => new { r.StoreId, r.Rating });

            builder.Entity<Review>()
                .HasIndex(r => new { r.FoodId, r.Rating });

            // Voucher
            builder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique(true);

            builder.Entity<Voucher>()
                .HasOne(v => v.Store)
                .WithMany(s => s.Vouchers)
                .HasForeignKey(v => v.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Voucher>()
                .HasIndex(v => new { v.IsActive, v.StartDate, v.EndDate });

            // VoucherUsage
            builder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Voucher)
                .WithMany(v => v.Usages)
                .HasForeignKey(vu => vu.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VoucherUsage>()
                .HasOne(vu => vu.User)
                .WithMany(u => u.VoucherUsages)
                .HasForeignKey(vu => vu.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Order)
                .WithOne(o => o.VoucherUsage)
                .HasForeignKey<VoucherUsage>(vu => vu.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<VoucherUsage>()
                .HasIndex(vu => new { vu.VoucherId, vu.UserId });

            // Favorite
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Store)
                .WithMany(s => s.Favorites)
                .HasForeignKey(f => f.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Food)
                .WithMany(f => f.Favorites)
                .HasForeignKey(f => f.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.StoreId })
                .IsUnique(true);

            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.FoodId })
                .IsUnique(true);

            // Notification
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

            base.OnModelCreating(builder);
        }
    }
}
