using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Projet_CryptoSim.MarketService.Data;

public class MarketDbContextFactory : IDesignTimeDbContextFactory<MarketDbContext>
{
    public MarketDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' manquante.");

        var optionsBuilder = new DbContextOptionsBuilder<MarketDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new MarketDbContext(optionsBuilder.Options);
    }
}