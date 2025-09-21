using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Tries to read user id from claims and convert it to int.
    /// Returns null if claim missing or not an integer.
    /// </summary>
    public static int? GetIntUserId(this ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        var idStr =
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(idStr, out var id) ? (int?)id : null;
    }
}
