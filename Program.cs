using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = $"http://0.0.0.0:{port}";
builder.WebHost.UseUrls(url);

var services = builder.Services;
var env = builder.Environment;

services.AddSwaggerGen();

services.AddApplicationServices(builder.Configuration);
// Add SignalR services
services.AddSignalR();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddIdentityServices(builder.Configuration);

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// 1. Forward headers from proxy so Request.Scheme is correct
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    // Всегда добавляем нужные CORS‑заголовки
    context.Response.Headers["Access-Control-Allow-Origin"] = "https://ng-dotnet.web.app";
    context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type,Authorization";
    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";

    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});


app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseRouting();
app.UseCors("CorsPolicy");

/* app.UseAuthentication();
app.UseAuthorization(); */

app.MapControllers();

using (var scopeRole = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scopeRole.ServiceProvider);
}

using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
var context = serviceProvider.GetRequiredService<UsersContext>();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
try
{
    await context.Database.MigrateAsync();
    // await StoreContextSeed.SeedAsync(context, loggerFactory);

    await identityContext.Database.MigrateAsync();
    //await StoreContextSeed.SeedAsync(context);
    await IdentityContextSeed.SeedUsersAsync(userManager);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
}
// global cors policy

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Map SignalR hubs
app.MapHub<ChatHub>("/hubs/chat");

// global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();

// UseAuthentication & UseAuthorization must comment, otherwise [Authorize] will cause 404s
//app.UseAuthentication();
//app.UseAuthorization();

app.Run();