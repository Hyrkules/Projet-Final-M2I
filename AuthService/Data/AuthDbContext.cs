using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace Projet_CryptoSim.AuthService.Data
{
    public class AuthDbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
