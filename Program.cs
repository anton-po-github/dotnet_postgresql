using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Bind to the correct port (supports Cloud Run) and configure URLs
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

// 11. DB migrations & seed data
using (var scope = app.Services.CreateScope())
{
    var servicesProvider = scope.ServiceProvider;
    var logger = servicesProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var usersContext = servicesProvider.GetRequiredService<UsersContext>();
        var identityContext = servicesProvider.GetRequiredService<IdentityContext>();
        var userManager = servicesProvider.GetRequiredService<UserManager<IdentityUser>>();

        await usersContext.Database.MigrateAsync();
        await identityContext.Database.MigrateAsync();
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
