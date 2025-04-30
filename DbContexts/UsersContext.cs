using Microsoft.EntityFrameworkCore;

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

