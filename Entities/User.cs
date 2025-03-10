
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

public class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? Phone { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Details { get; set; }
}

public class AppUser : IdentityUser
{
}

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class RegisterDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
}