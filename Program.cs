using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var env = builder.Environment;

services.AddSwaggerGen();

services.AddApplicationServices(builder.Configuration);
services.AddIdentityServices(builder.Configuration);

//services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // ignore omitted parameters on models to enable optional params (e.g. User update)
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
var context = serviceProvider.GetRequiredService<UsersContext>();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
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

app.Run();