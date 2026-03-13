using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;

namespace PortfolioService.Data;

public class PortfoliodbContext(DbContextOptions<PortfoliodbContext> options) : DbContext(options)
{
    public DbSet<Holding> Holdings => Set<Holding>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Holding>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.UserId).IsRequired();
            entity.Property(h => h.CryptoSymbol).HasColumnType("VARCHAR(10)").IsRequired();
            entity.HasIndex(h => new { h.UserId, h.CryptoSymbol }).IsUnique();
            entity.Property(h => h.Quantity).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(h => h.AverageBuyPrice).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(h => h.UpdatedAt).HasColumnType("DATETIME").IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.CryptoSymbol).HasColumnType("VARCHAR(10)").IsRequired();
            entity.Property(t => t.Type).HasColumnType("VARCHAR(10)").IsRequired();
            entity.Property(t => t.Quantity).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(t => t.PriceAtTime).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(t => t.Total).HasColumnType("DECIMAL(18,8)").IsRequired();
            entity.Property(t => t.ExecutedAt).HasColumnType("DATETIME").IsRequired();
        });
    }
}