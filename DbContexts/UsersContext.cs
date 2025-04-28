using dotnet_postgresql.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_postgresql.DbContexts
{
    public class UsersContext : DbContext
    {
        protected readonly IConfiguration _config;

        public UsersContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_config.GetConnectionString("UsersConnection"));
        }

        public DbSet<User> Users { get; set; }
    }
}
