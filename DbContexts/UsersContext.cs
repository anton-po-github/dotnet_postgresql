using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class UsersContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersContext(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var connStr = _configuration.GetConnectionString("UsersConnection");
            options.UseNpgsql(connStr);
        }
    }

    public DbSet<User> Users { get; set; } = null!;

    // Извлекаем currentUserId напрямую из HttpContext
    private string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        // Попытка достать стандартный claim NameIdentifier
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(id))

            return id;

        return user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Глобальный фильтр: OwnerId == текущий IdentityUserId
        var currentUserId = GetCurrentUserId();

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.OwnerId == currentUserId);

        base.OnModelCreating(modelBuilder);
    }

    // Подстановка OwnerId перед сохранением новых сущностей
    public override int SaveChanges()
    {
        ApplyOwnerId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyOwnerId();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyOwnerId()
    {
        var userId = GetCurrentUserId();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                     .Where(e => e.State == EntityState.Added))
        {
            entry.Entity.OwnerId = userId;
        }
    }
}

