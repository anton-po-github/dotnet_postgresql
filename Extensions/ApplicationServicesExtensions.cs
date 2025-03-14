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

        services.AddScoped<EmailService>();
        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();
        services.AddScoped<IBlobService, BlobService>();

        services.AddCors(opt =>
          {
              opt.AddPolicy("CorsPolicy", policy =>
              {
                  policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
                  // policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowAnyOrigin();
              });
          });

        return services;
    }
}
