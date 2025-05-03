using Microsoft.Extensions.Options;
using MongoDB.Driver;

public static class MongoServiceExtensions
{
    /// Registering MongoDB and MongoClient settings via Options Pattern
    public static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Binding configuration to a strongly-typed class via Options Pattern
        services
            .AddOptions<MongoDBSettings>()
            .Bind(configuration.GetSection("MongoDB"))
            .ValidateDataAnnotations() // Check attributes [Required], [Range], etc.
            .ValidateOnStart();         // Ensures that the application will not start with incorrect settings

        // 2. Register MongoClient as a singleton, using IOptions<MongoDBSettings>
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        // 3. (Optional) Register the MongoDBSettings class itself for direct injection
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<MongoDBSettings>>().Value
        );

        return services;
    }
}