using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "CorsPolicy",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200", "https://ng-dotnet.web.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

// 2. Swagger (Development only)
builder.Services.AddSwaggerGen();

// Disable default claim type mapping
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// 3. Application services
builder.Services.AddMongoServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddIdentityServices(builder.Configuration);

// 4. EF Core and Identity setup

var app = builder.Build();

// 5. Forwarded headers for correct scheme/host detection behind a proxy
app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    }
);

// 6. Global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");

// 7. HTTPS redirection and Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// 8. Enable routing (must come before CORS/Auth)
app.UseRouting();

// 9. Apply CORS policy (before authentication)
app.UseCors("CorsPolicy");

// 10. Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// 11. Top-level routing: Controllers and SignalR hubs
app.MapControllers(); // Maps attribute-routed controllers
app.MapHub<ChatHub>("/hubs/chat"); // Maps SignalR hub at /hubs/chat

// 12. Development only: Apply EF Core pending migrations
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        var contexts = new DbContext[]
        {
            sp.GetRequiredService<UsersContext>(),
            sp.GetRequiredService<IdentityContext>(),
            sp.GetRequiredService<PostgresContext>(),
            sp.GetRequiredService<ChatMessageContext>(),
        };

        foreach (var ctx in contexts)
        {
            var pending = ctx.Database.GetPendingMigrations();
            if (pending.Any())
            {
                logger.LogInformation(
                    "Applying {Count} pending migrations for {Context}",
                    pending.Count(),
                    ctx.GetType().Name
                );
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

// 13. Seed initial data (users, roles)
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
        await IdentityContextSeed.SeedUsersAsync(userManager);
        await IdentitySeeder.SeedRolesAsync(sp);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding");
    }
}

// 14. Start the application
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();
