using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class OrderdbContext(DbContextOptions<OrderdbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.CryptoSymbol).HasColumnType("VARCHAR(10)").IsRequired();
            entity.Property(o => o.Type).HasColumnType("VARCHAR(10)").HasConversion<string>();
            entity.Property(o => o.Quantity).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(o => o.Price).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(o => o.Total).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(o => o.Status).HasColumnType("VARCHAR(20)").HasConversion<string>();
            entity.Property(o => o.CreatedAt).HasColumnType("DATETIME").IsRequired();
            entity.Property(o => o.ExecutedAt).HasColumnType("DATETIME");
        });
    }
}