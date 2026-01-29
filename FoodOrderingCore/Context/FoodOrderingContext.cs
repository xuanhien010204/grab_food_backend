using FoodOrderingCore.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingCore.Context
{
    public class FoodOrderingContext : DbContext
    {
        public FoodOrderingContext(DbContextOptions<FoodOrderingContext> options) : base(options) { }

        public virtual DbSet<User> Users { set; get; }
        public virtual DbSet<Role> Roles { set; get; }
        public virtual DbSet<Store> Stores { set; get; }
        public virtual DbSet<Food> Foods { set; get; }
        public virtual DbSet<FoodSize> FoodSizes { get; set; }
        public virtual DbSet<FoodStore> FoodStores { set; get; }
        public virtual DbSet<Order> Orders { set; get; }
        public virtual DbSet<OrderDetail> OrderDetails { set; get; }
        public virtual DbSet<FoodType> FoodTypes { set; get; }
        public virtual DbSet<Tenant> Tenants { set; get; }
        public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

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

            base.OnModelCreating(builder);
        }
    }
}
