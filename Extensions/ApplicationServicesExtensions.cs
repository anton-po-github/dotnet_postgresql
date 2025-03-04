using Microsoft.EntityFrameworkCore;
public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        services.AddDbContext<IdentityContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("IdentityConnection"));
        });

        services.AddDbContext<UsersContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("UsersConnection"));
        });

        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();

        return services;
    }
}
