using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS
builder.Services.AddCors(options =>
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

// 2. Swagger (Dev only)
builder.Services.AddSwaggerGen();

// JWT claims mapping
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// 3. Application services
builder.Services.AddMongoServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddIdentityServices(builder.Configuration);

// 4. EF Core и Identity в сервисах

var app = builder.Build();

// 5. Forwarded headers for correct scheme/host detection
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 6. Global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");

// 7. HTTPS и Swagger в Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// 8. Включаем Routing (маршрутизация должна идти до CORS/Auth)
app.UseRouting();

// 9. CORS до Auth
app.UseCors("CorsPolicy");

// 10. Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// 11. Топ-левел маршрутизация: контроллеры и SignalR-хабы
app.MapControllers();                                        // Maps attribute-routed controllers
app.MapHub<ChatHub>("/hubs/chat");                          // Maps SignalR hub at /hubs/chat

// 12. Dev-only: автоматические миграции EF Core
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

// 13. Seed data (users, roles)
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

// 14. Launching the application
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();
