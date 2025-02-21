using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
public static class UserManagerExtensions
{
    /*   public static async Task<AppUser> FindUserByClaimsPrincipleWithAddressAsync(this UserManager<User> input, ClaimsPrincipal user)
      {
          var email = user.FindFirstValue(ClaimTypes.Email);

          return await input.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == email);
      } */

    public static async Task<AppUser> FindByEmailFromClaimsPrinciple(this UserManager<AppUser> input, ClaimsPrincipal user)
    {
        var email = user.FindFirstValue(ClaimTypes.Email);

        return await input.Users.SingleOrDefaultAsync(x => x.Email == email);
    }


}
