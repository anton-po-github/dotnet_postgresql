using Microsoft.AspNetCore.Identity;
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

        services.AddIdentity<AppUser, IdentityRole>(o => o.SignIn.RequireConfirmedEmail = true)
        .AddEntityFrameworkStores<IdentityContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<EmailService>();
        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();
        services.AddScoped<IBlobService, BlobService>();

        return services;
    }
}
