using System.Security.Claims;
using System.Text;
using dotnet_postgresql.DbContexts.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_postgresql.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            var builder = services.AddIdentityCore<IdentityUser>();

            builder = new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);
            builder.AddEntityFrameworkStores<IdentityContext>();
            builder.AddSignInManager<SignInManager<IdentityUser>>();
            builder.AddRoleManager<RoleManager<IdentityRole>>();
            builder.AddRoles<IdentityRole>();

            /*    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(opt =>
                  {
                      opt.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuerSigningKey = true,
                          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"])),
                          ValidIssuer = config["Token:Issuer"],
                          ValidateIssuer = true,
                          ClockSkew = TimeSpan.FromMinutes(1),
                          ValidateAudience = false
                      };
                  }); */

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {

                    // ваши TokenValidationParameters…
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            var token = ctx.Request.Headers["Authorization"]
                                            .FirstOrDefault()?.Split(" ").Last();
                            Console.WriteLine($"[JWT] OnMessageReceived. Token = {token?.Substring(0, 10)}…");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            Console.WriteLine($"[JWT] Успешно валидирован для {ctx.Principal.Identity.Name}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine($"[JWT] Ошибка аутентификации: {ctx.Exception.Message}");
                            return Task.CompletedTask;
                        }
                    };

                    options.SaveToken = true;
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Token:Issuer"],
                        ValidAudience = config["Token:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["Token:Key"])
                        )
                    };

                    var key = Encoding.UTF8.GetBytes(config["Token:Key"]);

                    //  opt.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                    // opt.TokenValidationParameters.RoleClaimType = "role";

                    /*   opt.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuerSigningKey = true,
                          IssuerSigningKey = new SymmetricSecurityKey(key),
                          ValidateIssuer = true,
                          ValidIssuer = config["Token:Issuer"],
                          ValidateAudience = true,
                          ValidAudience = config["Token:Audience"],
                          ValidateLifetime = true,
                          ClockSkew = TimeSpan.Zero
                      }; */
                });

            return services;
        }
    }
}
