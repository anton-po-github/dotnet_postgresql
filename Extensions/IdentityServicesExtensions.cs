using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

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
