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

        services.AddDbContext<PostgresContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("PostgresConnection"));
        });

        services.AddDbContext<ChatMessageContext>(x =>
        {
            x.UseNpgsql(config.GetConnectionString("ChatMessageConnection"));
        });

        services.AddScoped<EmailService>();
        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();
        services.AddScoped<PostgresService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped(typeof(IGenericService<>), (typeof(GenericService<>)));

        services.AddControllers();

        return services;
    }
}
