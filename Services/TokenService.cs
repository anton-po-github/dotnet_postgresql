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
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager;

    public TokenService(IConfiguration config, UserManager<IdentityUser> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    public async Task<string> CreateAccessTokenAsync(IdentityUser user)
    {
        var key = Encoding.UTF8.GetBytes(_config["Token:Key"]);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Token:Issuer"],
            audience: _config["Token:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            // expires: DateTime.UtcNow.AddMinutes(4),   // access токен – 15 мин
            expires: DateTime.UtcNow.AddYears(4),   // access токен – 15 мин
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
            ValidAudience = _config["Token:Audience"],
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


/* public class TokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<IdentityUser> _userManager;
    public TokenService(IConfiguration config, UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        _config = config;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
    }

    public async Task<string> CreateTokenAsync(IdentityUser user)
    {

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.UserName),
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokeDesc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = DateTime.UtcNow.AddYears(10),
            SigningCredentials = creds,
            Issuer = _config["Token:Issuer"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokeDesc);

        return tokenHandler.WriteToken(token);
    }
}
 */