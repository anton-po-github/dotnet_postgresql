using Microsoft.EntityFrameworkCore;

public class UsersContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public int? CurrentUserId { get; private set; }

    public UsersContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;

        CurrentUserId = _httpContextAccessor.HttpContext?.User.GetIntUserId();
    }

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

    /// Configures the EF Core model: adds a global filter
    /// by OwnerId so that each selection returns only its own records.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Here we do not call GetCurrentUserId() directly,
        // so that the filter will fill in the value of the CurrentUserId property dynamically.
        //var currentUserId = GetCurrentUserId();

        var currentUserId = CurrentUserId;

        modelBuilder
            .Entity<User>()
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
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyOwnerId();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// General logic for setting OwnerId for all new entities.
    /// Iterates through ChangeTracker and for each Added state
    /// sets OwnerId = current UserId.
    private void ApplyOwnerId()
    {
        // var userId = GetCurrentUserId();

        var userId = CurrentUserId;

        // If we don't have a numeric user id — skip assigning OwnerId
        if (!userId.HasValue)
            return;

        foreach (
            var entry in ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added)
        )
        {
            entry.Entity.OwnerId = userId;
        }
    }
}
