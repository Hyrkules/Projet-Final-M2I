using MarketService.Models;
using Microsoft.EntityFrameworkCore;

namespace Projet_CryptoSim.MarketService.Data;

public class MarketdbContext(DbContextOptions<MarketdbContext> options) : DbContext(options)
{
    public DbSet<Crypto> Cryptos => Set<Crypto>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Crypto>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Symbol).HasColumnType("VARCHAR(10)").IsRequired();
            entity.HasIndex(c => c.Symbol).IsUnique();
            entity.Property(c => c.Name).HasColumnType("VARCHAR(100)").IsRequired();
            entity.Property(c => c.CurrentPrice).HasColumnType("DECIMAL(18,8)");
            entity.Property(c => c.LastUpdated).HasColumnType("DATETIME");
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.CryptoSymbol).HasColumnType("VARCHAR(10)").IsRequired();
            entity.Property(p => p.Price).HasColumnType("DECIMAL(18,8)");
            entity.Property(p => p.RecordedAt).HasColumnType("DATETIME");
        });
    }
}