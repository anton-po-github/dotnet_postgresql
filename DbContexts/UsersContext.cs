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

    /// Property that dynamically returns the current IdentityUserId
    /// from HttpContext.User (claim NameIdentifier or "sub").
    public string? CurrentUserId =>
      _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    /// Configures the connection string to PostgreSQL,
    /// if the dependency graph is not already configured.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var connStr = _configuration.GetConnectionString("UsersConnection");
            options.UseNpgsql(connStr);
        }
    }

    public DbSet<User> Users { get; set; } = null!;

    /// Retrieves the current UserId from HttpContext.User,
    /// checking authentication and required claims.
    private string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        // If there is no context or not authenticated - return null
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        // Trying to get the standard claim NameIdentifier
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(id))

            return id;
        // If NameIdentifier is missing, we take "sub"
        return user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    /// Настраивает модель EF Core: добавляет глобальный фильтр
    /// по OwnerId, чтобы каждая выборка возвращала только свои записи.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Here we do not call GetCurrentUserId() directly,
        // so that the filter will fill in the value of the CurrentUserId property dynamically.
        var currentUserId = GetCurrentUserId();

        modelBuilder.Entity<User>()
                  // Use the CurrentUserId property. EF Core will insert it into SQL on every request :contentReference[oaicite:1]{index=1}
                  .HasQueryFilter(u => u.OwnerId == CurrentUserId);
        // Calling the base method is necessary to register any additional 
        // configurations that might have been defined in modules, plugins, or parent classes.
        base.OnModelCreating(modelBuilder);
    }

    /// Override SaveChanges to automatically fill in the OwnerId field for new entities before saving.
    public override int SaveChanges()
    {
        ApplyOwnerId();
        return base.SaveChanges();
    }

    /// Override the asynchronous SaveChangesAsync variant.
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyOwnerId();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// General logic for setting OwnerId for all new entities.
    /// Iterates through ChangeTracker and for each Added state
    /// sets OwnerId = current UserId.
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

