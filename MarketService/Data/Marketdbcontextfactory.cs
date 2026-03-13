using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Projet_CryptoSim.MarketService.Data;

public class MarketdbContextFactory : IDesignTimeDbContextFactory<MarketdbContext>
{
    public MarketdbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' manquante.");

        var optionsBuilder = new DbContextOptionsBuilder<MarketdbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new MarketdbContext(optionsBuilder.Options);
    }
}