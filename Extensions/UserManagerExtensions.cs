using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace dotnet_postgresql.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<IdentityUser> FindByEmailFromClaimsPrinciple(this UserManager<IdentityUser> input, ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await input.Users.SingleOrDefaultAsync(x => x.Email == email);
        }
    }
}