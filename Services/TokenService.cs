using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(IdentityUser user);
    Task<RefreshToken> CreateRefreshTokenAsync(string ipAddress, IdentityUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public TokenService(IConfiguration config, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;

        // Retrieve and validate the token key from configuration
        var tokenKey = config["Token:Key"]
                      ?? throw new InvalidOperationException("Configuration value 'Token:Key' is missing");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        // Read issuer and audience
        _issuer = config["Token:Issuer"]
                  ?? throw new InvalidOperationException("Configuration value 'Token:Issuer' is missing");
        _audience = _env.IsProduction()
                    ? "https://anton-posgres-app.azurewebsites.net"
                    : "http://127.0.0.1:8080";
    }

    public async Task<string> CreateAccessTokenAsync(IdentityUser user)
    {
        // Create signing credentials once
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        // Build claims
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // Create the JWT
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
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
            Expires = DateTime.UtcNow.AddDays(365),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = user.Id
        };
        return Task.FromResult(refreshToken);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = false // Ignore token lifetime check
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwt ||
            !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
