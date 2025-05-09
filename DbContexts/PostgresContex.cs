using Microsoft.EntityFrameworkCore;

public class PostgresContext : DbContext
{
    protected readonly IConfiguration _config;

    public PostgresContext(IConfiguration configuration)
    {
        _config = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_config.GetConnectionString("PostgresConnection"));
    }

    public DbSet<PostgresUsers> users { get; set; }
    public DbSet<PostgresProducts> products { get; set; }
    //public DbSet<PostgresBooks> books { get; set; }
    // public DbSet<PostgresGoods> goods { get; set; }
}

