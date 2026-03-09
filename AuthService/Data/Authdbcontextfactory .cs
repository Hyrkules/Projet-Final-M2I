using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Projet_CryptoSim.AuthService.Data;

namespace Projet_CryptoSim.AuthService.Data
{
    public class Authdbcontextfactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            // Lit appsettings.json pour récupérer la connection string
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' manquante.");

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
