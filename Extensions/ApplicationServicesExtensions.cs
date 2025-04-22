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

        // services.AddCors();

        services.AddCors(opt =>
         {
             opt.AddPolicy("CorsPolicyDev", policy =>
             {
                 policy.WithOrigins("http://localhost:4200")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // <-- This is important!
                                      //  policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
                                      // policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowAnyOrigin();
             });
         });

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicyProd", policy =>
            {
                policy.WithOrigins("https://ng-dotnet.web.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // <-- This is important!
                                     //  policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
                                     // policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowAnyOrigin();
            });
        });

        services.AddControllers();

        return services;
    }
}
