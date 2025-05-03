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

    /// Свойство, динамически возвращающее текущий IdentityUserId
    /// из HttpContext.User (claim NameIdentifier или "sub").
    public string? CurrentUserId =>
      _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    /// Конфигурирует строку подключения к PostgreSQL,
    /// если граф зависимостей ещё не настроен.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var connStr = _configuration.GetConnectionString("UsersConnection");
            options.UseNpgsql(connStr);
        }
    }

    public DbSet<User> Users { get; set; } = null!;

    /// Извлекает текущий UserId из HttpContext.User,
    /// проверяя аутентификацию и необходимые claims.
    private string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        // Если нет контекста или не аутентифицирован — вернём null
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        // Попытка достать стандартный claim NameIdentifier
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(id))

            return id;
        // Если NameIdentifier отсутствует — берём "sub"
        return user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    /// Настраивает модель EF Core: добавляет глобальный фильтр
    /// по OwnerId, чтобы каждая выборка возвращала только свои записи.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Здесь мы не вызываем GetCurrentUserId() напрямую,
        // чтобы фильтр подставлял значение свойства CurrentUserId динамически.
        var currentUserId = GetCurrentUserId();

        modelBuilder.Entity<User>()
                  // Используем свойство CurrentUserId. EF Core подставит его в SQL при каждом запросе :contentReference[oaicite:1]{index=1}
                  .HasQueryFilter(u => u.OwnerId == CurrentUserId);

        // Вызов базового метода необходим для регистрации
        // всех дополнительных конфигураций, которые могли быть
        // определены в модулях, плагинах или в родительских классах.
        base.OnModelCreating(modelBuilder);
    }

    /// Переопределяем SaveChanges, чтобы перед сохранением
    /// автоматически заполнить поле OwnerId у новых сущностей.
    public override int SaveChanges()
    {
        ApplyOwnerId();
        return base.SaveChanges();
    }

    /// Переопределяем асинхронный вариант SaveChangesAsync.
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyOwnerId();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// Общая логика по установке OwnerId для всех новых сущностей.
    /// Проходит по ChangeTracker и для каждого состояния Added
    /// назначает OwnerId = текущий UserId.
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

