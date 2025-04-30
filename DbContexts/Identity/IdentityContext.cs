using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    protected readonly IConfiguration _config;
    public IdentityContext(DbContextOptions<IdentityContext> options, IConfiguration config) : base(options)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_config.GetConnectionString("IdentityConnection"));
    }

    public DbSet<IdentityUser> Identity { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    //  public DbSet<UserProfile> UserProfiles { get; set; }
}

