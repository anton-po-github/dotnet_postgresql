using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityContext : IdentityDbContext<IdentityUser>
{
    protected readonly IConfiguration _config;
    public IdentityContext(DbContextOptions<IdentityContext> options, IConfiguration configuration) : base(options)
    {
        _config = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_config.GetConnectionString("IdentityConnection"));
    }

    public DbSet<IdentityUser> Identity { get; set; }

    /*  protected override void OnModelCreating(ModelBuilder builder)
     {
         base.OnModelCreating(builder);
     } */
}
