using Microsoft.AspNetCore.Identity;

public class IdentityContextSeed
{
    public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        if (!userManager.Users.Any())
        {
            var user = new AppUser
            {
                Email = "bob@test.com",
                UserName = "Bob",

            };

            await userManager.CreateAsync(user, "Pa$$w0rd");
        }
    }
}
