
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
        public virtual DbSet<FoodStore> FoodStores { set; get; }
        public virtual DbSet<Order> Orders { set; get; }
        public virtual DbSet<OrderDetail> OrderDetails { set; get; }
        public virtual DbSet<FoodType> FoodTypes { set; get; }
        public virtual DbSet<Tenant> Tenants { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Tenant - Store relationship (1:N)
            builder.Entity<Store>()
                .HasOne(s => s.Tenant)
                .WithMany(t => t.Stores)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // FoodStore relationships
            builder.Entity<FoodStore>()
                .HasIndex(fs => new { fs.StoreId, fs.FoodId })
                .IsUnique(true);

            builder.Entity<FoodStore>()
                .HasOne(fs => fs.Store)
                .WithMany(s => s.FoodStores)
                .HasForeignKey(fs => fs.StoreId);

            builder.Entity<FoodStore>()
                .HasOne(fs => fs.Food)
                .WithMany(f => f.FoodStores)
                .HasForeignKey(fs => fs.FoodId);

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

            // Seed data for Roles
            builder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Manager" },
                new Role { Id = 3, Name = "Admin" }
            );

            // Seed data for FoodTypes
            builder.Entity<FoodType>().HasData(
                new FoodType { Id = 1, Name = "Appetizer" },
                new FoodType { Id = 2, Name = "Main Course" },
                new FoodType { Id = 3, Name = "Dessert" },
                new FoodType { Id = 4, Name = "Beverage" }
            );

            // Seed data for Tenants
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
