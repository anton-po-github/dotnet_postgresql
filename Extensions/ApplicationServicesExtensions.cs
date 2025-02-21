using Microsoft.EntityFrameworkCore;
public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        services.AddDbContext<AppIdentityDbContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("IdentityConnection"));
        });

        services.AddDbContext<DataContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();

        return services;
    }
}
