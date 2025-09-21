using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var builder = services.AddIdentityCore<IdentityUser>();

        builder = new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);
        builder.AddEntityFrameworkStores<IdentityContext>();
        builder.AddSignInManager<SignInManager<IdentityUser>>();
        builder.AddRoleManager<RoleManager<IdentityRole>>();
        builder.AddRoles<IdentityRole>();

        var tokenKey =
            config["Token:Key"]
            ?? throw new InvalidOperationException(
                "The 'Token:Key' value is missing from the configuration."
            );
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx
                            .Request.Headers["Authorization"]
                            .FirstOrDefault()
                            ?.Split(" ")
                            .Last();
                        Console.WriteLine(
                            $"[JWT] OnMessageReceived. Token = {token?.Substring(0, 10)}…"
                        );
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        Console.WriteLine(
                            $"[JWT] Успешно валидирован для {ctx.Principal?.Identity?.Name}"
                        );
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine($"[JWT] Ошибка аутентификации: {ctx.Exception.Message}");
                        return Task.CompletedTask;
                    },
                };

                options.SaveToken = true;
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Token:Issuer"],
                    ValidAudience = config["Token:Audience"],
                    IssuerSigningKey = signingKey,
                };
            });

        return services;
    }
}
