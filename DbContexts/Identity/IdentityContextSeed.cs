using Microsoft.AspNetCore.Identity;

public class IdentityContextSeed
{
    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        if (!userManager.Users.Any())
        {
            var user = new IdentityUser { Email = "bob@test.com", UserName = "Bob" };

            await userManager.CreateAsync(user, "Pa$$w0rd");
        }
    }
}
