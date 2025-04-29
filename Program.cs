using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using MongoDB.Driver;
using dotnet_postgresql.Hubs;
using dotnet_postgresql.mongoDB;
using dotnet_postgresql.Extensions;
using dotnet_postgresql.Errors;
using dotnet_postgresql.DbContexts;
using dotnet_postgresql.DbContexts.Identity;
using dotnet_postgresql.Helpers;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var services = builder.Services;
var configuration = builder.Configuration;

// 1. Configure CORS to allow both local dev and production Angular apps
services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://ng-dotnet.web.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 2. Swagger/OpenAPI (only in Development)
services.AddSwaggerGen();

var mongoSettings = builder.Configuration
    .GetSection("MongoDB")
    .Get<MongoDBSettings>();
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoSettings.ConnectionString));

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
// либо
//JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); 


// 3. Application services, AutoMapper, SignalR, Identity, etc.
services.AddApplicationServices(configuration);
services.AddSignalR();
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.AddIdentityServices(configuration);

// 4. EF Core DbContexts, Identity, etc. inside AddApplicationServices / AddIdentityServices

var app = builder.Build();

// 5. Forwarded headers for correct Request.Scheme behind proxies
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 6. Global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");

// 7. Routing
app.UseRouting();

// 8. Enable CORS BEFORE Authentication/Authorization
app.UseCors("CorsPolicy");

// 9. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 10. Map controllers and hubs
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/hubs/chat");
});

// 12. Dev-only: automatic EF Core migrations
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        // Apply pending migrations only in Development environment
        var contexts = new DbContext[]
        {
            sp.GetRequiredService<UsersContext>(),
            sp.GetRequiredService<IdentityContext>(),
            sp.GetRequiredService<PostgresContext>(),
            sp.GetRequiredService<ChatMessageContext>()
        };

        foreach (var ctx in contexts)
        {
            var pending = ctx.Database.GetPendingMigrations();
            if (pending.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations for {Context}", pending.Count(), ctx.GetType().Name);
                ctx.Database.Migrate();
                logger.LogInformation("Migrations applied to {Context}", ctx.GetType().Name);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations in Development");
    }
}

// 11. DB migrations & seed data
using (var scope = app.Services.CreateScope())
{
    var servicesProvider = scope.ServiceProvider;
    var logger = servicesProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var userManager = servicesProvider.GetRequiredService<UserManager<IdentityUser>>();

        await IdentityContextSeed.SeedUsersAsync(userManager);
        await IdentitySeeder.SeedRolesAsync(servicesProvider);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding");
    }
}

// 12. Swagger UI & HTTPS redirection in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.Run();
