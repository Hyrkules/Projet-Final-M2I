using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace Projet_CryptoSim.AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            //Contrainte : Email unique pour les utilisateurs
            // pas deux utilisateurs avec le même email
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}
