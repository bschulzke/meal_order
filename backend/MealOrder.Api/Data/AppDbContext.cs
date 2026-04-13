using Microsoft.EntityFrameworkCore;
using MealOrder.Api.Models;

namespace MealOrder.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderDiscount> OrderDiscounts { get; set; }
    public DbSet<OrderTax> OrderTaxes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderDiscount>()
            .HasOne(od => od.Discount)
            .WithMany()
            .HasForeignKey(od => od.DiscountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderTax>()
            .HasOne(ot => ot.Tax)
            .WithMany()
            .HasForeignKey(ot => ot.TaxId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
