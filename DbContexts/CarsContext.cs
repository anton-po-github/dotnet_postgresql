using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;

public class CarsContext : DbContext
{
    protected readonly IConfiguration _config;

    public CarsContext(IConfiguration configuration)
    {
        _config = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_config.GetConnectionString("CarsConnection"));
    }

    public DbSet<Car> cars { get; set; } = null!;
}
