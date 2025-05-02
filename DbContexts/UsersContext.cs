using Microsoft.EntityFrameworkCore;

public class UsersContext : DbContext
{
    protected readonly IConfiguration _config;
    private readonly ICurrentUserService _currentUserService;

    public UsersContext(IConfiguration configuration, ICurrentUserService currentUserService)
    {
        _config = configuration;
        _currentUserService = currentUserService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_config.GetConnectionString("UsersConnection"));
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Глобальный фильтр по OwnerId
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.OwnerId == _currentUserService.UserId);

        // Прочие конфигурации...
    }
}

