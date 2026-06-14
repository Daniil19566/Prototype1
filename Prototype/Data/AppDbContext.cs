using Microsoft.EntityFrameworkCore;
using Prototype.Models;

namespace Prototype.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<StockOperation> StockOperations => Set<StockOperation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();
            modelBuilder.Entity<User>().Property(x => x.Role).HasConversion<string>();
            modelBuilder.Entity<Order>().HasIndex(x => x.OrderNumber).IsUnique();
            modelBuilder.Entity<Order>().Property(x => x.Status).HasConversion<string>();

            modelBuilder.Entity<Product>().Property(x => x.QuantityInStock).HasDefaultValue(0);
            modelBuilder.Entity<Product>().ToTable(x => x.HasCheckConstraint("CK_Products_NonNegativeStock", "\"QuantityInStock\" >= 0"));


            modelBuilder.Entity<OrderItem>()
                .HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockOperation>()
                .Property(x => x.OperationType)
                .HasConversion<string>();

            modelBuilder.Entity<StockOperation>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
