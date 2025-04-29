using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using dotnet_postgresql.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_postgresql.Services
{
    public interface ITokenService
    {
        Task<string> CreateAccessTokenAsync(IdentityUser user);
        Task<RefreshToken> CreateRefreshTokenAsync(string ipAddress, IdentityUser user);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public TokenService(IConfiguration config, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _config = config;
            _userManager = userManager;
            _env = env;
        }

        public async Task<string> CreateAccessTokenAsync(IdentityUser user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Token:Key"]);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _config["Token:Issuer"],
                audience: _env.IsProduction()
                ? "https://dotnet-postgresql-service-864171160719.us-central1.run.app"
                : "http://127.0.0.1:8080",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<RefreshToken> CreateRefreshTokenAsync(string ipAddress, IdentityUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),     // refresh – на 7 дней
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = user.Id
            };
            return Task.FromResult(refreshToken);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_config["Token:Key"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _env.IsProduction()
                ? "https://dotnet-postgresql-service-864171160719.us-central1.run.app"
                : "http://127.0.0.1:8080",
                ValidateIssuer = true,
                ValidIssuer = _config["Token:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false          // игнорируем срок жизни
            };

            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}